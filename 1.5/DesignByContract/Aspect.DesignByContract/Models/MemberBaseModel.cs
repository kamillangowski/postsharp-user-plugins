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
using Aspect.DesignByContract.Interface;

namespace Aspect.DesignByContract.Models
{
	/// <summary>
	/// Basisklasse für Modelle die in Member serialisiert werden.
	/// </summary>
	[Serializable]
	internal class MemberBaseModel
	{

		#region Interne Variablen (11) 

		/// <summary>
		/// Membervariable der Eigenschaft ContractAssembly.
		/// </summary>
		private byte[] mContractAssembly = null;
		/// <summary>
		/// Membervariable der Eigenschaft ContractClassName.
		/// </summary>
		private string mContractClassName = string.Empty;
		/// <summary>
		/// Membervariable der Eigenschaft ContractObject.
		/// </summary>
		[NonSerialized]
		private IContract mContractObject = null;
		/// <summary>
		/// Membervariable der Eigenschaft ContractsAreEqual.
		/// </summary>
		private bool mContractsAreEqual = false;
		/// <summary>
		/// Membervariable der Eigenschaft DbcAccessType.
		/// </summary>
		private AccessType mDbcAccessType = AccessType.Both;
		/// <summary>
		/// Membervariable der Eigenschaft DbcCheckTime.
		/// </summary>
		private CheckTime mDbcCheckTime = CheckTime.Both;
		/// <summary>
		/// Membervariable der Eigenschaft ExceptionString.
		/// </summary>
		[NonSerialized]
		private string mExceptionString = string.Empty;
		/// <summary>
		/// Membervariable der Eigenschaft ExceptionType.
		/// </summary>
		[NonSerialized]
		private Type mExceptionType = null;
		/// <summary>
		/// Membervariable der Eigenschaft ClassGenericTypes.
		/// </summary>
		[NonSerialized]
		private Type[] mGenericClassTypes = null;
		/// <summary>
		/// Membervariable der Eigenschaft ExpressionTypesLoaded.
		/// </summary>
		private bool mGenericTypesLoaded = false;
		/// <summary>
		/// Membervariable der Eigenschaft Member.
		/// </summary>
		[NonSerialized]
		private MemberInfo mMember = null;

		#endregion Interne Variablen 

		#region Eigenschaften (11) 

		/// <summary>
		/// Beinhaltet die Assembly mit allen definierten Kontrakten.
		/// </summary>
		internal byte[] ContractAssembly
		{
			get { return mContractAssembly; }
			set { mContractAssembly = value; }
		}

		/// <summary>
		/// Gibt den Name der Kontraktklasse zurück
		/// </summary>
		internal string ContractClassName
		{
			get { return mContractClassName; }
			set { mContractClassName = value; }
		}

		/// <summary>
		/// Das Objekt über das die Kontrakte angesprochen werden können.
		/// </summary>
		internal IContract ContractObject
		{
			get { return mContractObject; }
			set { mContractObject = value; }
		}

		/// <summary>
		/// Gibt an ob der Ein und Austrittkontrakt gleich sind.
		/// </summary>
		internal bool ContractsAreEqual
		{
			get { return mContractsAreEqual; }
			set { mContractsAreEqual = value; }
		}

		/// <summary>
		/// Gibt an, bei welchem Zugriff der Kontrakt angewendet werden soll.
		/// </summary>
		internal AccessType DbcAccessType
		{
			get { return mDbcAccessType; }
			set { mDbcAccessType = value; }
		}

		/// <summary>
		/// Gibt an, zu welchem Zeitpunkt der Kontrakt angewendet werden soll.
		/// </summary>
		public CheckTime DbcCheckTime
		{
			get { return mDbcCheckTime; }
			set { mDbcCheckTime = value; }
		}

		/// <summary>
		/// Gibt den Exceptiontext an, der bei Nichterfüllung des Kontrakts geworfen werden soll.
		/// </summary>
		internal string ExceptionString
		{
			get { return mExceptionString; }
			private set { mExceptionString = value; }
		}

		/// <summary>
		///  Gibt den Typ der Exception an, die bei Nichterfüllung des Kontrakts geworfen werden soll.
		/// </summary>
		internal Type ExceptionType
		{
			get { return mExceptionType; }
			private set { mExceptionType = value; }
		}

		/// <summary>
		/// Gibt eine Liste der generischen Typen zurück
		/// </summary>
		public Type[] GenericClassTypes
		{
			get { return mGenericClassTypes; }
			set { mGenericClassTypes = value; }
		}

		/// <summary>
		/// Gibt zurück of die generischen Typen (sofern vorhanden) der Instanz, die den Kontrakt
		/// prüft, bereits geladen wurde.
		/// </summary>
		internal bool GenericTypesLoaded
		{
			get { return mGenericTypesLoaded; }
			set { mGenericTypesLoaded = value; }
		}

		/// <summary>
		/// Gibt das Element zurück an dem der Kontrakt definiert wurde.
		/// </summary>
		internal MemberInfo Member
		{
			get { return mMember; }
			set { mMember = value; }
		}

		#endregion Eigenschaften 

		#region Kon/Destructoren (Dispose) (1) 

		/// <summary>
		/// Konstruktor
		/// </summary>
		/// <param name="member">Das Element an dem der DbcAspect definiert wurde.</param>
		/// <param name="accessType">Gibt an, bei welchem Zugriff der Kontrakt angewendet werden soll.</param>
		/// <param name="checkTime">Gibt an, zu welchem Zeitpunkt der Kontrakt angewendet werden soll.</param>
		/// <param name="contractClassName">Gibt den Namen der Kontraktklasse an.</param>
		/// <param name="exceptionType">Gibt den Typ der Exception an, die bei Nichterfüllung des Kontrakts geworfen werden soll.</param>
		/// <param name="exceptionString">Gibt den Exceptiontext an, der bei Nichterfüllung des Kontrakts geworfen werden soll.</param>
		internal MemberBaseModel(MemberInfo member, AccessType accessType, CheckTime checkTime, string contractClassName, Type exceptionType, string exceptionString)
		{
			DbcAccessType = accessType;
			DbcCheckTime = checkTime;
			Member = member;
			ContractClassName = contractClassName;
			ExceptionType = exceptionType;
			ExceptionString = exceptionString;
		}

		#endregion Kon/Destructoren (Dispose) 


		#region Felder die nur im DebugModus benötigt werden

#if (DEBUG)
		/// <summary>
		/// Membervariable der Eigenschaft PdbFile.
		/// </summary>
		private byte[] mPdbFile = null;
		/// <summary>
		/// Die PDB Datei die für das Debuggen benötigt wird.
		/// </summary>
		internal byte[] PdbFile
		{
			get { return mPdbFile; }
			set { mPdbFile = value; }
		}
		/// <summary>
		/// Membervariable der Eigenschaft SourceCodeFile.
		/// </summary>
		private byte[] mSourceCodeFile = null;
		/// <summary>
		/// Datei die den Sourcecode enthält mit dem die Kontrakt-Assembly erzeugt wurde.
		/// </summary>
		internal byte[] SourceCodeFile
		{
			get { return mSourceCodeFile; }
			set { mSourceCodeFile = value; }
		}
#endif
		#endregion
	}
}
