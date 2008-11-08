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

namespace Aspect.DesignByContract.Controller
{
	/// <summary>
	/// (Singleton) Prüft ob ein Element ein Aspekt zugeordnet werden darf.
	/// </summary>
	internal class AspectController
	{

		#region Interne Variablen (4) 

		/// <summary>
		/// Liste mit allen Elementen den der Dbc Aspekt hinzugefügt wurde.
		/// </summary>
		List<MemberInfo> mAspectingMembers = new List<MemberInfo>();
		/// <summary>
		/// Eine Dictionary in der Festgehalten wird für welche Assembly welcher Kontraktklassenname
		/// gewählt wurde.
		/// </summary>
		private Dictionary<Assembly, string> mContractClassNameDictionary = new Dictionary<Assembly, string>();
		/// <summary>
		/// Singletonmember
		/// </summary>
		private static AspectController mInstance = null;
		/// <summary>
		/// Zum Locken damit nicht 2 mal eine Instanz von der Klasse
		/// AspectController erzeugt wird.
		/// </summary>
		private static object mLockObject = new object();

		#endregion Interne Variablen 

		#region Eigenschaften (1) 

		/// <summary>
		/// Singleton Eigenschaft
		/// </summary>
		internal static AspectController Instance
		{
			get
			{
				if (mInstance != null)
					return mInstance;
				lock (mLockObject)
				{
					if (mInstance == null)
						mInstance = new AspectController();
					return mInstance;
				}
			}
		}

		#endregion Eigenschaften 

		#region Kon/Destructoren (Dispose) (1) 

		/// <summary>
		/// Konstruktor
		/// </summary>
		private AspectController()
		{
		}

		#endregion Kon/Destructoren (Dispose) 

		#region Methoden (2) 

		/// <summary>
		/// Prüft, ob ein Member den Dbc Aspect hinzugefügt werden kann.
		/// </summary>
		/// <param name="member">Member der geprüft werden soll.</param>
		/// <returns>
		/// true=>Der Aspekt kann hinzugefügt werden. 
		/// false=>Der Aspekt kann nicht hinzugefügt werden.
		/// </returns>
		internal bool CanAttachAspectToMember(MemberInfo aspectingMember)
		{
			if (mAspectingMembers.Contains(aspectingMember))
				return false;
			mAspectingMembers.Add(aspectingMember);
			return true;
		}

		/// <summary>
		/// Gibt den Klassennamen zurück, unter dem die Kontraktklasse für den Übergabeparameter erreichbar ist.
		/// </summary>
		/// <param name="assembly">Assembly in der Kontrakte definiert wurden.</param>
		/// <returns>Name der Kontraktklasse</returns>
		internal string CreateContractClassName(Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException(Resources.ExcAssemblyNull);
			if (mContractClassNameDictionary.ContainsKey(assembly))
				return mContractClassNameDictionary[assembly];
			string contractClassName = Resources.StrContractClassPrefix + Guid.NewGuid().ToString().Replace("-", "");
			mContractClassNameDictionary.Add(assembly, contractClassName);
			return contractClassName;
		}

		#endregion Methoden 

	}
}
