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
using System.IO;
using System.Globalization;
using Aspect.DesignByContract.Interface;
using Aspect.DesignByContract.Properties;

namespace Aspect.DesignByContract.Controller
{
	/// <summary>
	/// Singleton der die ContractAssemblies zur Laufzeit verwalten soll.
	/// </summary>
	internal class ContractController
	{
		/// <summary>
		/// Liste mit allen IContract Objekten.
		/// </summary>
		private Dictionary<string, IContract> mAssemblyList = null;

		/// <summary>
		/// Zum Locken damit nicht 2 mal eine Instanz von der Klasse
		/// ContractManager erzeugt wird.
		/// </summary>
		private static object mLockObject = new object();
		/// <summary>
		/// Singletonmember
		/// </summary>
		private static ContractController mInstance = null;

		/// <summary>
		/// Konstruktor
		/// </summary>
		private ContractController()
		{
		}

		/// <summary>
		/// Singleton Eigenschaft
		/// </summary>
		internal static ContractController Instance
		{
			get
			{
				if (mInstance != null)
					return mInstance;
				lock (mLockObject)
				{
					if (mInstance == null)
						mInstance = new ContractController();
					return mInstance;
				}
			}
		}

		/// <summary>
		/// Erzeugt einen Dateinamen der wie die vContractedAssembly heisst.
		/// Lediglich die vFileNameExtension, die auch die Dateiendung enthält, wird angefügt.
		/// </summary>
		/// <param name="contractedAssembly">Assembly die die Kontrakte beinhaltet</param>
		/// <param name="fileNameExtension">Namenserweiterung (plus Dateiendung)</param>
		/// <returns>Der Neue Dateiname mit kompletten Pfad</returns>
		internal string CreateFileName(Assembly contractedAssembly, string fileNameExtension)
		{
			return Path.Combine(Path.GetDirectoryName(contractedAssembly.Location), Path.GetFileNameWithoutExtension(contractedAssembly.Location) + fileNameExtension);
		}

		/// <summary>
		/// Erzeugt eine Datei aus dem übergebenen Byte Array.
		/// </summary>
		/// <param name="fileName">Dateiname wo die Datei gespeichert werden soll.</param>
		/// <param name="fileArray">Byte Array aus dem die Datei erzeugt werden soll.</param>
		private void CreateFile(string fileName, byte[] fileArray)
		{
			if (File.Exists(fileName))
				File.Delete(fileName);
			using (FileStream fileStream = File.Create(fileName))
			{
				fileStream.Write(fileArray, 0, fileArray.Length);
			}
		}


		/// <summary>
		/// Läd eine Assembly und erzeugt die Dateien die zum Debuggen benötigt werden.
		/// </summary>
		/// <param name="contractedAssembly">Die Assembly in der die Kontrakte definiert sind.</param>
		/// <param name="assemblyKey">Schlüssel unter dem die Kontraktklasse gefunden werden kann.</param>
		/// <param name="contractAssembly">Die Assembly die geladen werden soll.</param>
		/// <param name="debugInformation">Die DebugInformationen (*.pdb) Datei.</param>
		/// <param name="sourceCode">Die SourceCode (*.cs) Datei.</param>
		internal void LoadContractAssembly(Assembly contractedAssembly, string assemblyKey, byte[] contractAssembly, byte[] debugInformation, byte[] sourceCode)
		{
			// SourceCode Datei erzeugen (im Debugverzeichnis)
			CreateFile(CreateFileName(contractedAssembly, Resources.StrSourceCodeFielExt),
				sourceCode);
			// PDB Datei erzeugen (im Debugverzeichnis)
			CreateFile(CreateFileName(contractedAssembly, Resources.StrPdbFileExt),
				debugInformation);
			// DLL Datei erzeugen (im Debugverzeichnis)
			string assemblyName = CreateFileName(contractedAssembly, Resources.StrAssemblyFileExt);
			CreateFile(assemblyName,
				contractAssembly);
			Assembly assembly = Assembly.LoadFile(assemblyName);

			if (mAssemblyList == null)
				mAssemblyList = new Dictionary<string, IContract>();

			mAssemblyList.Add(assemblyKey, CreateIContractObject(assembly));
		}

		/// <summary>
		/// Läd die Assembly die die Kontraktklasse beinhaltet die die IContract Schnittstelle implementiert.
		/// </summary>
		/// <param name="assemblyKey">Schlüssel unter dem die Kontraktklasse gefunden werden kann.</param>
		/// <param name="contractAssembly">Die Assembly die geladen werden soll.</param>
		internal void LoadContractAssembly(string assemblyKey, byte[] contractAssembly)
		{
			Assembly assembly = Assembly.Load(contractAssembly);

			if (mAssemblyList == null)
				mAssemblyList = new Dictionary<string, IContract>();

			mAssemblyList.Add(assemblyKey, CreateIContractObject(assembly));
		}

		/// <summary>
		/// Gibt das Objekt vom Typ IContract zurück.
		/// </summary>
		/// <param name="key">Schlüssel unter dem das Objekt abgelegt ist.</param>
		/// <returns>Das Objekt vom Typ IContract.</returns>
		internal IContract GetContractObject(string key)
		{
			if (!mAssemblyList.ContainsKey(key))
				throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.ExcContractClassNotExist, key));
			if (mAssemblyList[key] == null)
				throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.ExcContractClassIsNull, key));

			return mAssemblyList[key];
		}


		/// <summary>
		/// Prüft die übergebene Assembly und erzeugt das Objekt welches die IContract Schnittstelle implementiert.
		/// </summary>
		/// <param name="contractAssembly">Die Assembly die das IContract Objekt beinhaltet.</param>
		/// <returns>Das aus der Assembly erzeugt IContract Objekt.</returns>
		private IContract CreateIContractObject(Assembly contractAssembly)
		{
			if (contractAssembly.GetExportedTypes().Length != 1)
				throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.ExcMoreClassesInContractAssembly, contractAssembly.FullName));

			Type aMethodType = contractAssembly.GetExportedTypes()[0];
			if (aMethodType.IsSubclassOf(typeof(IContract)))
				throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.ExcNoIContractClass, contractAssembly.FullName));


			object aInstance = Activator.CreateInstance(aMethodType);
			return aInstance as IContract;
		}
	}
		
}
