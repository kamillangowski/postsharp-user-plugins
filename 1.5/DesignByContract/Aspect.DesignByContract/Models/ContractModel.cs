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
using Aspect.DesignByContract.Properties;

namespace Aspect.DesignByContract.Models
{
	/// <summary>
	/// Modell mit den allgemeinen Informationen zu dem Kontrakt.
	/// </summary>
	[Serializable]
	internal class ContractModel
	{

		#region Interne Variablen (7) 

		/// <summary>
		/// Membervariable der Eigenschaft OriginalContract.
		/// </summary>
		[NonSerialized]
		private string mContract = string.Empty;
		/// <summary>
		/// Membervariable der Eigenschaft ContractKey.
		/// </summary>
		private string mContractKey = string.Empty;
		/// <summary>
		/// Membervariable der Eigenschaft ConvertedContract.
		/// </summary>
		[NonSerialized]
		private string mConvertedContract = string.Empty;
		/// <summary>
		/// Membervariable der Eigenschaft GetOldValueKey.
		/// </summary>
		private string mGetOldValueKey = string.Empty;
		/// <summary>
		/// Membervariable der Eigenschaft GetOldValuesStatements.
		/// </summary>
		[NonSerialized]
		private string[] mGetOldValuesStatements = null;
		/// <summary>
		/// Membervariable der Eigenschaft OldValueExist.
		/// </summary>
		private bool mOldValueExist = false;

		#endregion Interne Variablen 

		#region Eigenschaften (7) 

		/// <summary>
		/// Der Kontakt wie er übergeben wurde.
		/// </summary>
		internal string Contract
		{
			get { return mContract; }
			private set { mContract = value; }
		}

		/// <summary>
		/// Der eindeutige Schlüssel des Kontrakts.
		/// </summary>
		internal string ContractKey
		{
			get { return mContractKey;}
			private set { mContractKey = value;}
		}

		/// <summary>
		/// Gibt den Konvertierten Contract zurück, oder setzt ihn.
		/// </summary>
		internal string ConvertedContract
		{
			get { return mConvertedContract; }
			set { mConvertedContract = value; }
		}

		/// <summary>
		/// Der eindeutige Schlüssel um alte Werte zu laden
		/// </summary>
		internal string GetOldValueKey
		{
			get { return mGetOldValueKey; }
			private set { mGetOldValueKey = value; }
		}

		/// <summary>
		/// Gibt die Statements zurück für das Laden der Alten Werte die bei Austritt evtl verglichen werden sollen.
		/// </summary>
		public string[] GetOldValuesStatements
		{
			get { return mGetOldValuesStatements; }
			set { mGetOldValuesStatements = value; }
		}

		/// <summary>
		/// Gibt an ob bei Methoden eintritt ein alter Wert geladen werden muss.
		/// </summary>
		internal bool OldValueExist
		{
			get { return mOldValueExist; }
			private set { mOldValueExist = value; }
		}

		#endregion Eigenschaften 

		#region Kon/Destructoren (Dispose) (1) 

		/// <summary>
		/// Konstruktor
		/// </summary>
		/// <param name="contract">Der Kontrakt</param>
		internal ContractModel(string contract)
		{
			// Kontrakt speichern
			Contract = contract;

			// Schlüssel erzeugen
			string key = Guid.NewGuid().ToString().Replace("-", "");

			ContractKey = Resources.StrContractMethodPrefix + key;

			if (contract.IndexOf(Resources.StrAspectParameterOldAccess) >= 0)
			{
				// Es müssen alte werte geladen werden.
				GetOldValueKey = Resources.StrGetOldValueMethodPrefix + key;
				OldValueExist = true;
			}

		}

		#endregion Kon/Destructoren (Dispose) 

	}
}
