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
using Aspect.DesignByContract.Enums;

namespace Aspect.DesignByContract.Models
{
	/// <summary>
	/// Beinhaltet alle Daten die für eine Methode relevant sind.
	/// </summary>
	[Serializable]
	internal class MethodModel : MemberBaseModel
	{

		#region Interne Variablen (2) 

		/// <summary>
		/// Membervariable der Eigenschaft EnsureContract.
		/// </summary>
		private ContractModel mEnsureContract = null;
		/// <summary>
		/// Membervariable der Eigenschaft RequireContract.
		/// </summary>
		private ContractModel mRequireContract = null;

		#endregion Interne Variablen 

		#region Eigenschaften (2) 

		/// <summary>
		/// Model mit den Werten zum Prüfen der Austrittsbedingung.
		/// </summary>
		internal ContractModel EnsureContract
		{
			get { return mEnsureContract; }
			private set { mEnsureContract = value; }
		}

		/// <summary>
		/// Model mit den Werten zum Prüfen der Eintrittsbedingung.
		/// </summary>
		internal ContractModel RequireContract
		{
			get { return mRequireContract; }
			set { mRequireContract = value; }
		}

		#endregion Eigenschaften 

		#region Kon/Destructoren (Dispose) (1) 

		/// <summary>
		/// Konstruktor
		/// </summary>
		/// <param name="requireContract">Der Kontrakt zur Eintrittsprüfung.</param>
		/// <param name="ensureContract">Der Kontrakt zur Austrittsprufung.</param>
		/// <param name="member">Das Element an dem der DbcAspect definiert wurde</param>
		/// <param name="accessType">Gibt an, bei welchem Zugriff der Kontrakt angewendet werden soll.</param>
		/// <param name="checkTime">Gibt an, zu welchem Zeitpunkt der Kontrakt angewendet werden soll.</param>
 		/// <param name="contractClassName">Gibt den Namen der Kontraktklasse an.</param>
		/// <param name="exceptionType">Gibt den Typ der Exception an, die für den Nichterfüllungsfall geworfen werden soll.</param>
		/// <param name="exceptionString">Gibt den Exceptiontext an, der bei Nichterfüllung geworfen werden soll.</param>
		internal MethodModel(string requireContract, string ensureContract, MemberInfo member, AccessType accessType, CheckTime checkTime, string contractClassName, Type exceptionType, string exceptionString)
			: base(member, accessType, checkTime, contractClassName, exceptionType, exceptionString)
		{
			// Erzeugen und zuweisen der ContractModels.
			if (requireContract == ensureContract)
			{
				RequireContract = new ContractModel(requireContract);
				EnsureContract = RequireContract;
				ContractsAreEqual = true;
				return;
			}
			if (!string.IsNullOrEmpty(requireContract))
				RequireContract = new ContractModel(requireContract);
			if (!string.IsNullOrEmpty(ensureContract))
				EnsureContract = new ContractModel(ensureContract);
			
		}

		#endregion Kon/Destructoren (Dispose) 

	}
}
