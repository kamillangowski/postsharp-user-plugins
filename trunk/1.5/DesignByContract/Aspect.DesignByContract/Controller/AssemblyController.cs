/*

Copyright (c) 2008, Patrick Jahnke

All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of the Patrick Jahnke nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Aspect.DesignByContract.Properties;
using System.Globalization;
using System.IO;

namespace Aspect.DesignByContract.Controller
{
	/// <summary>
	/// Singleton für das Verwalten der Assemlby die die Kontrakte enthält.
	/// </summary>
	internal class AssemblyController
	{

		#region Interne Variablen (5) 

		/// <summary>
		/// Singletonmember
		/// </summary>
		private static AssemblyController mInstance = null;
		/// <summary>
		/// Eine Liste mit allen Typen die doppelt Definiert sind.
		/// </summary>
		private Dictionary<string, List<Type>> mDoubleExistingTypes = new Dictionary<string, List<Type>>();
		/// <summary>
		/// Eine List mit allen bereits geladene Assemblies (Key) + Referenzen (Value)
		/// </summary>
		private Dictionary<string, List<Assembly>> mReferencedAssemblies = new Dictionary<string, List<Assembly>>();
		/// <summary>
		/// Zum Locken damit nicht 2 mal eine Instanz von der Klasse
		/// AssemblyManager erzeugt wird.
		/// </summary>
		private static object mLockObject = new object();
		/// <summary>
		/// Eine Liste mit den TypeNamen (value) und den Typen (value)
		/// </summary>
		private Dictionary<string, Type> mTypeList = new Dictionary<string, Type>();

		#endregion Interne Variablen 

		#region Eigenschaften (1) 

		/// <summary>
		/// Singleton Eigenschaft
		/// </summary>
		internal static AssemblyController Instance
		{
			get
			{
				if (mInstance != null)
					return mInstance;
				lock (mLockObject)
				{
					if (mInstance == null)
						mInstance = new AssemblyController();
					return mInstance;
				}
			}
		}

		#endregion Eigenschaften 

		#region Kon/Destructoren (Dispose) (1) 

		/// <summary>
		/// Konstruktor
		/// </summary>
		private AssemblyController()
		{
		}

		#endregion Kon/Destructoren (Dispose) 

		#region Methoden (4) 


		/// <summary>
		/// Sucht nach dem Typ mit dem Namen typeName.
		/// </summary>
		/// <param name="typeName">Name des Typs der gefunden werden soll.</param>
		/// <param name="assembly">Assembly in der auf diesen Typ zugegriffen werden soll.</param>
		/// <returns>Typ der typeName entspricht oder null.</returns>
		internal Type FindTypeInAssemblyAndAllReferences(string typeName, Assembly assembly)
		{
			// Sorfern die Assembly noch nicht geladen wurde, bitte nachholen und alle
			// referenzen auch.
			if (!mReferencedAssemblies.ContainsKey(assembly.FullName))
				mReferencedAssemblies.Add(assembly.FullName, LoadReferences(assembly));

			// Exception werfen wenn der Typ doppelt vorkommt
			if (mDoubleExistingTypes.ContainsKey(typeName))
			{
				string exceptionString = string.Empty;
				foreach (Type doubleType in mDoubleExistingTypes[typeName])
				{
					exceptionString += "\r\nType:" + doubleType.Namespace + "." + doubleType.Name
						+ "\tAssembly:" + doubleType.Assembly.FullName;
				}
				throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.ExcAccessToDoubleType, new Object[] { typeName, exceptionString }));
			}

			// Typ suchen
			if (mTypeList.ContainsKey(typeName))
				return mTypeList[typeName];

			// Kein Typ gefunden
			return null;

		}

		/// <summary>
		/// Gibt eine Liste der referenzierten Assemlies von assembly zurück.
		/// </summary>
		/// <param name="assembly">Assembly dessen Referenzen geladen werden sollen.</param>
		/// <returns>Liste der referenzierten Assemblies.</returns>
		internal Assembly[] GetReferencedAssemblies(Assembly assembly)
		{
			if (!mReferencedAssemblies.ContainsKey(assembly.FullName))
				mReferencedAssemblies.Add(assembly.FullName, LoadReferences(assembly));
			Assembly[] loadedAssemblies = new Assembly[mReferencedAssemblies[assembly.FullName].Count];
			int i = 0;
			foreach (Assembly loadedAssembly in mReferencedAssemblies[assembly.FullName])
			{
				loadedAssemblies[i] = loadedAssembly;
				i++;
			}
			return loadedAssemblies;
		}

		/// <summary>
		/// Gibt alle Referenzen von assembly zurück.
		/// </summary>
		/// <param name="assembly">Assembly dessen Referenzen gefunden werden sollen.</param>
		/// <returns>Liste mit allen Referenzen</returns>
		private List<Assembly> LoadReferences(Assembly assembly)
		{
			List<Assembly> referencedAssemblies = new List<Assembly>();
			List<string> loadedAssemblies = new List<string>();

			referencedAssemblies.Add(assembly);
			loadedAssemblies.Add(Path.GetFileName(assembly.Location).ToUpper());
			//Typen der Assembly laden
			LoadTypes(assembly.GetTypes());

			// alle geladenen ReferenzAssemblies hinzufügen
			foreach (AssemblyName referenceAssemblyName in assembly.GetReferencedAssemblies())
			{
				try
				{
					Assembly loadedAssembly = Assembly.Load(referenceAssemblyName);
					loadedAssemblies.Add(Path.GetFileName(loadedAssembly.Location).ToUpper());
					referencedAssemblies.Add(loadedAssembly);
					//Typen der Assembly laden
					LoadTypes(loadedAssembly.GetExportedTypes());
				}
				// Ein leerer Cacheblock da die Datei anscheinend doch keine Assembly war.
				catch { }
			}

			// Jetzt noch alle Referenzen laden die im bin Verzeichnis liegen (ausser die 
			// *.vshost.EXTENSION) da eine Referenz erst dann als Referenz gilt wenn ein 
			// Typ aus dieser Assembly definiert ist.
			string referencePath = Path.GetDirectoryName(assembly.Location).Replace(@"\obj\", @"\bin\");
			// BUG: Das Verzeichnis \Before-PostSharp muss entfernt werden -> kam mit PostSharp 1.5
			referencePath = referencePath.Replace(@"\Before-PostSharp", "");
			DirectoryInfo referenceDirectory = new DirectoryInfo(referencePath);
			foreach (FileInfo referenceFile in referenceDirectory.GetFiles())
			{
				if (((referenceFile.Extension.ToUpper() == ".DLL")
					|| (referenceFile.Extension.ToUpper() == ".EXE"))
					&& (!loadedAssemblies.Contains(referenceFile.Name.ToUpper()))
					&& (!referenceFile.Name.EndsWith(".vshost" + referenceFile.Extension)))
				{
					try
					{
						Assembly loadedAssembly = Assembly.LoadFile(referenceFile.FullName);
						loadedAssemblies.Add(referenceFile.Name.ToUpper());
						referencedAssemblies.Add(loadedAssembly);
						//Typen der Assembly laden
						LoadTypes(loadedAssembly.GetExportedTypes());
					}
					// Ein leerer Cacheblock da die Datei anscheinend doch keine Assembly war.
					catch { }
				}
			}
			return referencedAssemblies;
		}

		/// <summary>
		/// Ladet die Typen in den Cache und prüft ob evtl. Typen mit gleichen Namen vorhanden sind.
		/// </summary>
		/// <param name="types">Typen die dem Cache hinzugefügt werden sollen.</param>
		private void LoadTypes(Type[] types)
		{
			foreach (Type assemblyType in types)
			{
				if (mDoubleExistingTypes.ContainsKey(assemblyType.Name))
				{
					mDoubleExistingTypes[assemblyType.Name].Add(assemblyType);
				}
				else if (mTypeList.ContainsKey(assemblyType.Name))
				{
					List<Type> doubleList = new List<Type>();
					doubleList.Add(assemblyType);
					doubleList.Add(mTypeList[assemblyType.Name]);
					mDoubleExistingTypes.Add(assemblyType.Name, doubleList);
				}
				else
					mTypeList.Add(assemblyType.Name, assemblyType);
			}
		}

		#endregion Methoden 

	}
}
