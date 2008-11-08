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
using Aspect.DesignByContract.Enums;
using System.Reflection;

namespace Aspect.DesignByContract.Models
{
	[Serializable]
	internal class FieldModel : MemberBaseModel
	{

		#region Interne Variablen (2) 

		/// <summary>
		/// Membervariable der Eigenschaft GetContract.
		/// </summary>
		private ContractModel mGetContract = null;
		/// <summary>
		/// Membervariable der Eigenschaft SetContract.
		/// </summary>
		private ContractModel mSetContract = null;

		#endregion Interne Variablen 

		#region Eigenschaften (2) 

		/// <summary>
		/// Model mit den Werten zum Prüfen des Kontrakts bei Lesezugriff.
		/// </summary>
		public ContractModel GetContract
		{
			get { return mGetContract; }
			set { mGetContract = value; }
		}

		/// <summary>
		/// Model mit den Werten zum Prüfen des Kontrakts bei Schreibzugriff.
		/// </summary>
		public ContractModel SetContract
		{
			get { return mSetContract; }
			set { mSetContract = value; }
		}

		#endregion Eigenschaften 

		#region Kon/Destructoren (Dispose) (1) 

		/// <summary>
		/// Konstruktor
		/// </summary>
		/// <param name="getContract">Der Kontrakt der beim Lesezugriff geprüft wird.</param>
		/// <param name="setContract">Der Kontrakt der beim Schreibzugriff geprüft wird.</param>
		/// <param name="member">Das Element an dem der DbcAspect definiert wurde</param>
		/// <param name="accessType">Gibt an, bei welchem Zugriff der Kontrakt angewendet werden soll.</param>
		/// <param name="checkTime">Gibt an, zu welchem Zeitpunkt der Kontrakt angewendet werden soll.</param>
		/// <param name="contractClassName">Gibt den Namen der Kontraktklasse an.</param>
		/// <param name="exceptionType">Gibt den Typ der Exception an, die für den Nichterfüllungsfall geworfen werden soll.</param>
		/// <param name="exceptionString">Gibt den Exceptiontext an, der bei Nichterfüllung geworfen werden soll.</param>
		internal FieldModel(string getContract, string setContract, MemberInfo member, AccessType accessType, CheckTime checkTime, string contractClassName, Type exceptionType, string exceptionString)
			: base(member, accessType, checkTime, contractClassName, exceptionType, exceptionString)
		{
			// Erzeugen und zuweisen der ContractModels.
			if (getContract == setContract)
			{
				GetContract = new ContractModel(getContract);
				SetContract = GetContract;
				ContractsAreEqual = true;
				return;
			}
			if (!string.IsNullOrEmpty(getContract))
				GetContract = new ContractModel(getContract);
			if (!string.IsNullOrEmpty(setContract))
				SetContract = new ContractModel(setContract);
			
		}

		#endregion Kon/Destructoren (Dispose) 

	}
}
