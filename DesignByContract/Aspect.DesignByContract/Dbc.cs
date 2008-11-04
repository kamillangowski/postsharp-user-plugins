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
using PostSharp.Extensibility;
using Aspect.DesignByContract.Enums;
using Aspect.DesignByContract.MessageSources;
using PostSharp.Laos;
using System.Reflection;
using Aspect.DesignByContract.Properties;
using Aspect.DesignByContract.Exceptions;
using Aspect.DesignByContract.Controller;
using Aspect.DesignByContract.SubAspects;
using Aspect.DesignByContract.Models;

namespace Aspect.DesignByContract
{
	/// <summary>
	/// Design by Contract Aspekt.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
	[MulticastAttributeUsage(MulticastTargets.Field | MulticastTargets.Method | MulticastTargets.Property)]
	public class Dbc : CompoundAspect
	{

		#region Interne Variablen (5) 

		/// <summary>
		/// Liste mit den Kontrakten die im Konstruktor übergeben wurden.
		/// 4 Elemente bedeuten :
		/// [0] requireSetContract
		/// [1] requireGetContract
		/// [2] ensureSetContract
		/// [3] ensureGetContract
		/// 2 Einträge bedeuten bei Feldern:
		/// [0] getContract
		/// [1] setContract
		/// bei Methoden
		/// [0] requireContract
		/// [1] ensureContract
		/// bei Eigenschaften DbcAccessType == Both && DbcCheckTime == Both
		/// [0] requireSetContract && requireGetContract
		/// [1] ensureSetContract && ensureGetContract
		/// bei Eigenschaften DbcAccessType != Both
		/// [0] requireContract
		/// [1] ensureContract
		/// 1 Feld Einträge bestimmen die Parameter DbcAccessTye und DbcCheckTime die Benutzung.
		/// </summary>
		[NonSerialized]
		private List<string> mContracts = null;
		/// <summary>
		/// Membervariable der Eigenschaft DbcAccessType.
		/// </summary>
		[NonSerialized]
		private AccessType mDbcAccessType = AccessType.Both;
		/// <summary>
		/// Membervariable der Eigenschaft DbcCheckTime.
		/// </summary>
		[NonSerialized]
		private CheckTime mDbcCheckTime = CheckTime.Both;
		/// <summary>
		/// Membervariable der Eigenschaft DbcExceptionString.
		/// </summary>
		private string mDbcExceptionString = string.Empty;
		/// <summary>
		/// Membervariable der Eigenschaft DbcExceptionType.
		/// </summary>
		private Type mDbcExceptionType = null;

		#endregion Interne Variablen 

		#region Eigenschaften (4) 

		/// <summary>
		///  Gibt an, bei welchen Zugriff eine Eigenschaft oder Feld geprüft werden soll.
		/// </summary>
		public AccessType DbcAccessType
		{
			get { return mDbcAccessType; }
			set { mDbcAccessType = value; }
		}

		/// <summary>
		///  Gibt an, bei ob bei Eintritt bzw. Austritt eine Eigenschaft oder Methode geprüft werden soll.
		/// </summary>
		public CheckTime DbcCheckTime
		{
			get { return mDbcCheckTime; }
			set { mDbcCheckTime = value; }
		}

		/// <summary>
		/// Gibt den Exceptiontext an, der bei Nichterfüllung des Kontrakts geworfen werden soll.
		/// </summary>
		public string DbcExceptionString
		{
			get { return mDbcExceptionString; }
			set { mDbcExceptionString = value; }
		}

		/// <summary>
		///  Gibt den Typ der Exception an, die bei Nichterfüllung des Kontrakts geworfen werden soll.
		/// </summary>
		public Type DbcExceptionType
		{
			get { return mDbcExceptionType; }
			set { mDbcExceptionType = value; }
		}

		#endregion Eigenschaften 

		#region Kon/Destructoren (Dispose) (3) 

		/// <summary>
		/// Dieser Konstruktor dient Eigenschaften. Mit get und set Methode um eine Eintrittsbedingung 
		/// für die get bzw. set Methode und eine Austrittsbedingung für die get bzw. set Methode
		/// zu definieren.
		/// </summary>
		/// <param name="requireSetContract">Eintrittsbedingung für den set Teil einer Eigenschaft.</param>
		/// <param name="requireGetContract">Eintrittsbedingung für den get Teil einer Eigenschaft.</param>
		/// <param name="ensureSetContract">Austrittsbedingung für den set Teil einer Eigenschaft.</param>
		/// <param name="ensureGetContract">Austrittsbedingung für den get Teil einer Eigenschaft.</param>
		public Dbc ( string requireSetContract, string requireGetContract, string ensureSetContract, string ensureGetContract )
		{
			mContracts = new List<string>();
			mContracts.Add(requireSetContract);
			mContracts.Add(requireGetContract);
			mContracts.Add(ensureSetContract);
			mContracts.Add(ensureGetContract);
		}

		/// <summary>
		/// Dieser Konstruktor akzeptiert jedes Element. Bei Feldern gilt : "firstContract" wird bei lesenden 
		/// Zugriff (get) und  "secondContract" bei schreibenden Zugriff (set) geprüft. Bei Methoden und
		/// Eigenschaften gilt: "firstContract" wird bei Eintritt und "secondContract" bei Austritt geprüft.
		/// Sollten Sie bei einer Eigenschaft nur die get oder set Bedingung prüfen wollen, so benutzen Sie
		/// bitte den benannten Parameter "DbcAccessOption". Wenn Sie bei einer Eigenschaft verschiedene 
		/// Eintritts- bzw. Austrittsprüfungen definieren möchten, so benutzen Sie den Konstruktor mit vier 
		/// Contract Eingabemöglichkeiten und lassen Sie die nicht zu prüfende Kontrakte leer.
		/// </summary>
		/// <param name="firstContract">Prüft bei Feldern den Lesezugriff (get) und bei Methoden und Eigenschaften den Eintritt (require)</param>
		/// <param name="secondContract">Prüft bei Feldern den Schreibzugriff (set) und bei Methoden und Eigenschaften den Austritt (ensure)</param>
		public Dbc(string firstContract, string secondContract)
		{
			mContracts = new List<string>();
			mContracts.Add(firstContract);
			mContracts.Add(secondContract);
		}

		/// <summary>
		/// Dieser Konstruktor akzeptiert jedes Element. Für Felder gilt: get und set Zugriffe werden 
		/// mit "contract" geprüft sofern der benannte Parameter "DbcAccessType" den Wert "Both" hat. 
		/// Für Methoden gilt: require und ensure werden mit "contract" geprüft sofern der benannte
		/// Parameter "DbcCheckTime" den Wert "Both" hat.
		/// Für Eigenschaften gilt: sowohl get und set als auch require und ensure werden mit "contract"
		/// geprüft sofern die benannten Parameter "DbcAccessOption" und "DbcCheckTime" den Wert "Both"
		/// hat.
		/// </summary>
		/// <param name="contract">
		/// Bei Elementen werden alle möglichen Prüfungen vorgenommen, sofern nicht durch die benannten 
		/// Parameter "DbcAccessType" und "DbcCheckTime" anders angegeben
		/// </param>
		public Dbc(string contract)
		{
			mContracts = new List<string>();
			mContracts.Add(contract);
		}

		#endregion Kon/Destructoren (Dispose) 

		#region Methoden (13) 

		/// <summary>
		/// Fügt einem Feld den Contract Aspekt hinzu.
		/// </summary>
		/// <param name="contractList">Liste mit den übergebenen Kontrakten.</param>
		/// <param name="aspectCollection">Collection mit allen Membern denen der Aspekt zugewiesen wurde.</param>
		/// <param name="method">FieldInfo Objekt des Feldes an dem der Kontrakt definiert wurde.</param>
		private void AddAspectToField(List<string> contractList, LaosReflectionAspectCollection aspectCollection, FieldInfo field)
		{
			// Klassennamen erzeugen
			string contractClassName = AspectController.Instance.CreateContractClassName(field.DeclaringType.Assembly);

			FieldModel fieldModel = null;

			// Kontraktmodell erzeugen:
			string getContract = string.Empty;
			string setContract = string.Empty;
			if (contractList.Count == 2)
			{
				getContract = contractList[0];
				setContract = contractList[1];
			}
			else if (contractList.Count == 1)
			{
				getContract = contractList[0];
				setContract = contractList[0];
			}

			switch (DbcAccessType)
			{
				case AccessType.OnlyOnGet:
					fieldModel = new FieldModel(getContract, string.Empty, field, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
					break;
				case AccessType.OnlyOnSet:
					fieldModel = new FieldModel(string.Empty, setContract, field, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
					break;
				default:
					fieldModel = new FieldModel(getContract, setContract, field, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
					break;
			}

			aspectCollection.AddAspect(field, new FieldAccessAspect(fieldModel));
		}

		/// <summary>
		/// Fügt einer Methode den Contract Aspekt hinzu.
		/// </summary>
		/// <param name="contractList">Liste mit den übergebenen Kontrakten.</param>
		/// <param name="aspectCollection">Collection mit allen Membern denen der Aspekt zugewiesen wurde.</param>
		/// <param name="method">MethodInfo Objekt der Methode an der der Kontrakt definiert wurde.</param>
		private void AddAspectToMethod(List<string> contractList, LaosReflectionAspectCollection aspectCollection, MethodInfo method)
		{
			// Klassennamen erzeugen
			string contractClassName = AspectController.Instance.CreateContractClassName(method.DeclaringType.Assembly);

			MethodModel contractModel = null;

			// Kontraktmodell erzeugen:
			string requireContract = string.Empty;
			string ensureContract = string.Empty;
			if (contractList.Count == 2)
			{
				requireContract = contractList[0];
				ensureContract = contractList[1];
			}
			else if (contractList.Count == 1)
			{
				requireContract = contractList[0];
				ensureContract = contractList[0];
			}

			switch (DbcCheckTime)
			{
				case CheckTime.OnlyRequire:
					contractModel = new MethodModel(requireContract, string.Empty, method, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
					break;
				case CheckTime.OnlyEnsure:
					contractModel = new MethodModel(string.Empty, ensureContract, method, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
					break;
				default:
					contractModel = new MethodModel(requireContract, ensureContract, method, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
					break;
			}

			aspectCollection.AddAspect(method, new MethodBoundaryAspect(contractModel));
		}

		/// <summary>
		/// Fügt einer Eigenschaft den Contract Aspekt hinzu.
		/// </summary>
		/// <param name="contractList">Liste mit den übergebenen Kontrakten.</param>
		/// <param name="aspectCollection">Collection mit allen Membern denen der Aspekt zugewiesen wurde.</param>
		/// <param name="property">PropertyInfo Objekt der Eigenschaft an der der Kontrakt definiert wurde.</param>
		private void AddAspectToProperty(List<string> contractList, LaosReflectionAspectCollection aspectCollection, PropertyInfo property)
		{
			// Klassennamen erzeugen
			string contractClassName = AspectController.Instance.CreateContractClassName(property.DeclaringType.Assembly);

			MethodModel getContractModel = null;
			MethodModel setContractModel = null;

			// Kontraktmodelle erzeugen:
			if (contractList.Count == 4)
			{

				setContractModel = new MethodModel(contractList[0], contractList[2], property, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
				getContractModel = new MethodModel(contractList[1], contractList[3], property, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
			}
			else
			{
				string requireContract = string.Empty;
				string ensureContract = string.Empty;
				if (contractList.Count == 2)
				{
					requireContract = contractList[0];
					ensureContract = contractList[1];
				}
				else if (contractList.Count == 1)
				{
					requireContract = contractList[0];
					ensureContract = contractList[0];
				}

				switch (DbcCheckTime)
				{
					case CheckTime.OnlyRequire:
						setContractModel = new MethodModel(requireContract, string.Empty, property, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
						getContractModel = new MethodModel(requireContract, string.Empty, property, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
						break;
					case CheckTime.OnlyEnsure:
						setContractModel = new MethodModel(string.Empty, ensureContract, property, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
						getContractModel = new MethodModel(string.Empty, ensureContract, property, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
						break;
					default:
						setContractModel = new MethodModel(requireContract, ensureContract, property, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
						getContractModel = new MethodModel(requireContract, ensureContract, property, DbcAccessType, DbcCheckTime, contractClassName, DbcExceptionType, DbcExceptionString);
						break;
				}
			}


			// Die Aspekte den Get bzw. Set Methoden zuweisen.

			if ((property.GetGetMethod(true) != null) && (DbcAccessType != AccessType.OnlyOnSet))
				aspectCollection.AddAspect(property.GetGetMethod(true), new MethodBoundaryAspect(getContractModel));

			if ((property.GetSetMethod(true) != null) && (DbcAccessType != AccessType.OnlyOnGet))
				aspectCollection.AddAspect(property.GetSetMethod(true), new MethodBoundaryAspect(setContractModel));
		}

		/// <summary>
		/// Prüft ob die Kontrakte in contractList für element gültig sind.
		/// </summary>
		/// <param name="element">Element welchen den Aspect definiert.</param>
		/// <param name="contractList">Liste mit übergebenen Konrakten.</param>
		private void CheckContracts(MemberInfo element, List<string> contractList)
		{
			if (contractList == null)
				throw new ContractCompilerExcpetion(Resources.ExcContractListIsNull);

			// Der DbcExceptionTyp muss eine Ableitung von System.Exception sein.
			if ((DbcExceptionType != null) && (!DbcExceptionType.IsSubclassOf(typeof(System.Exception))))
				throw new ContractCompilerExcpetion(Resources.ExcNotValidDbcExceptionType);

			// Vier Kontrakte können nur an Eigenschaften definiert werden.
			if ((!(element is PropertyInfo)) && (contractList.Count == 4))
				throw new ContractCompilerExcpetion(Resources.ExcTooManyContractsDefined);

			// Wenn weniger als vier Kontrakte vorhanden sind darf kein Kontrakt Leer sein
			if (contractList.Count < 4)
			{
				foreach (string contract in contractList)
				{
					if (string.IsNullOrEmpty(contract))
						throw new ContractCompilerExcpetion(Resources.ExcEmptyContract);
				}
			}

			if (element is FieldInfo)
			{
				CheckFieldInfo((FieldInfo)element, contractList);
				return;
			}

			// Zwei Kontrakte können nur an Eigenschaften und Methoden definiert werden wenn DbcCheckTime = Both ist.
			if ((contractList.Count == 2) && ((element is PropertyInfo) || (element is MethodInfo)) && (DbcCheckTime != CheckTime.Both))
				throw new ContractCompilerExcpetion(Resources.ExcWrongDbcCheckTimeUseOnMethodAndProperty);


			if (element is PropertyInfo)
			{
				CheckPropertyInfo((PropertyInfo)element, contractList);
				return;
			}

			if (element is MethodInfo)
			{
				CheckMethodInfo((MethodInfo)element, contractList);
				return;
			}
		}

		/// <summary>
		/// Prüft ob die Kontrakte in contractList für das Feld field gültig sind.
		/// </summary>
		/// <param name="field">Feld welches den Aspect definiert.</param>
		/// <param name="contractList">Liste mit übergebenen Konrakten.</param>
		private void CheckFieldInfo(FieldInfo field, List<string> contractList)
		{
			// Zwei Kontrakte können nur an Felder definiert werden wenn DbcAccessTime = Both ist.
			if ((contractList.Count == 2) && (DbcAccessType != AccessType.Both))
				throw new ContractCompilerExcpetion(Resources.ExcWrongDbcAccessTypeUseOnField);

			// Felder dürfen nicht den benannten Parameter DbcCheckTime setzen.
			if (DbcCheckTime != CheckTime.Both)
				throw new ContractCompilerExcpetion(Resources.ExcWrongDbcCheckTimeUse);

			foreach (string contract in contractList)
			{
				// Der Apekt-Parameter '[old]' kann nicht an Felder definiert werden da ein Feld 
				// nur bei Zugriffen (lese oder schreibzugriff) den Kontrakt prüfen kann. 
				if ((!string.IsNullOrEmpty(contract)) && (contract.IndexOf(Resources.StrAspectParameterOldAccess) >= 0))
					throw new ContractCompilerExcpetion(string.Format(Resources.ExcOldAccesNotValidOnField, Resources.StrAspectParameterOldAccess));
				// Der Aspekt-Parameter '[result]' darf nicht verwendet werden wenn der Kontrakt
				// auch bei Schreibzugriff (set) geprüft werden soll.
				if ((!string.IsNullOrEmpty(contract)) && (DbcAccessType != AccessType.OnlyOnGet) && (contract.IndexOf(Resources.StrAspectParameterResultAccess) >= 0))
					throw new ContractCompilerExcpetion(string.Format(Resources.ExcResultNotValidOnField, Resources.StrAspectParameterResultAccess));
				// Der Aspekt-Parameter '[value]' darf nicht verwendet werden wenn der Kontrakt
				// auch bei Lesezugriff (get) geprüft werden soll.
				if ((!string.IsNullOrEmpty(contract)) && (DbcAccessType != AccessType.OnlyOnSet) && (contract.IndexOf(Resources.StrAspectParameterValueAccess) >= 0))
					throw new ContractCompilerExcpetion(string.Format(Resources.ExcValueNotValidOnField, Resources.StrAspectParameterValueAccess));
			}
		}

		/// <summary>
		/// Prüft ob die Kontrakte in contractList für die Methode method gültig sind.
		/// </summary>
		/// <param name="method">Element welchen den Aspect definiert.</param>
		/// <param name="contractList">Liste mit übergebenen Konrakten.</param>
		private void CheckMethodInfo(MethodInfo method, List<string> contractList)
		{
			// Methoden dürfen nicht den benannten Parameter DbcAccessType setzen.
			if (DbcAccessType != AccessType.Both)
				throw new ContractCompilerExcpetion(Resources.ExcWrongDbcAccessTypeUse);

			// Zwei Kontrakte können nur an Methoden definiert werden wenn DbcCheckTime = Both ist.
			if ((contractList.Count == 2) && (DbcCheckTime != CheckTime.Both))
				throw new ContractCompilerExcpetion(Resources.ExcWrongDbcCheckTimeUseOnMethod);

			// prüfen ob der old Parameter richtig gesetzt wurde:
			CheckOldParameter(GetRequireContracts(contractList));

			// prüfen ob der result Parameter richtig gesetzt wurde:
			if (method.ReturnType == typeof(void))
				CheckResultParameter(contractList);
			else if (contractList.Count == 2)
			{
				List<string> requireList = new List<string>();
				requireList.Add(contractList[0]);
				CheckResultParameter(requireList);
			}

			// prüfen ob der value Parameter richtig gesetzt wurde:
			CheckValueParameter(contractList);
		}

		/// <summary>
		/// Prüft die richtige Verwendung des Old Parameters bei Methoden und Eigenschaften.
		/// </summary>
		/// <param name="requireContractList">Alle Kontrakte die bei Eintritt geprüft werden.</param>
		private void CheckOldParameter(List<string> requireContractList)
		{
			// Der Apekt-Parameter '[old]' kann nicht an Felder definiert werden da ein Feld 
			// nur bei Zugriffen (Lese oder Schreibzugriff) den Kontrakt prüfen kann. 
			foreach (string contract in requireContractList)
			{
				// Der Apekt-Parameter '[old]' kann nicht in Eintrittskontrakte definiert werden da 
				// es zu diesem Zeitpunkt keinen alten Wert gibt.
				if ((!string.IsNullOrEmpty(contract)) && (contract.IndexOf(Resources.StrAspectParameterOldAccess) >= 0))
					throw new ContractCompilerExcpetion(string.Format(Resources.ExcOldAccesNotValidOnMethodAndProperty, Resources.StrAspectParameterOldAccess));
			}
		}

		/// <summary>
		/// Prüft ob die Kontrakte in contractList für die Eigenschaft property gültig sind.
		/// </summary>
		/// <param name="property">Element welchen den Aspect definiert.</param>
		/// <param name="contractList">Liste mit übergebenen Konrakten.</param>
		private void CheckPropertyInfo(PropertyInfo property, List<string> contractList)
		{
			// Es wurden vier Kontrakte definiert. Aus diesem Grund sind die benannten Paramter 
			// DbcAccessType und DbcCheckTime nicht von Bedeutung. Bitte Löschen Sie die Parameter 
			// um Missverständnisse zu vermeiden.
			if ((contractList.Count == 4) && ((DbcAccessType!= AccessType.Both) || (DbcCheckTime!= CheckTime.Both)))
				throw new ContractCompilerExcpetion(Resources.ExcTooManyContractsDefined);

			// Es werden keine vier Kontrakte benötigt, um lediglich ein
			// Lesezugriff (get) bzw. ein Schreibzugriff (set) zu prüfen.
			if ((contractList.Count == 4) && ((property.GetSetMethod(true) == null) || (property.GetGetMethod(true) == null)))
				throw new ContractCompilerExcpetion(Resources.ExcTooManyContractsOnGetOrSet);

			// prüfen ob der old Parameter richtig gesetzt wurde:
			CheckOldParameter(GetRequireContracts(contractList));

			List<string> checkList = new List<string>();
			if (contractList.Count == 4)
			{
				// Sie haben lediglich einen Kontrakt definiert für diesen Fall benutzen Sie bitte den 
				// Kontruktor mit einem Kontrakt als Übergabeparameter und setzen Sie die benannten 
				// Parameter DbcAccessType und DbcCheckTime.
				int contractCount = 0;
				foreach (string contract in checkList)
				{
					if (string.IsNullOrEmpty(contract))
						contractCount++;
				}
				if (contractCount >= 3)
					throw new ContractCompilerExcpetion(Resources.ExcOnlyOneContract);

				checkList.AddRange(new string[] { contractList[0], contractList[2] });
				// Prüfen ob es überhaupt Set Kontrakte gibt sonst ist der Konstruktor falsch.
				foreach (string contract in checkList)
				{
					if (string.IsNullOrEmpty(contract))
						throw new ContractCompilerExcpetion(Resources.ExcNoSetContracts);
				}
				// Prüfen ob der result Parameter richtig gesetzt wurde:
				CheckResultParameter(checkList);

				// Prüfen ob es überhaupt Get Kontrakte gibt sonst ist der Konstruktor falsch.
				checkList.Clear();
				checkList.AddRange(new string[] { contractList[1], contractList[3] });
				foreach (string contract in checkList)
				{
					if (string.IsNullOrEmpty(contract))
						throw new ContractCompilerExcpetion(Resources.ExcNoGetContracts);
				}

			}
			else if ((contractList.Count < 4) && (property.GetSetMethod(true) != null) && (DbcAccessType != AccessType.OnlyOnGet))
			{
				checkList.Add(contractList[0]);
				CheckResultParameter(checkList);
			}

			// Prüfen ob der value Parameter richtig gesetzt wurde:
			checkList = new List<string>();
			if (contractList.Count == 4)
			{
				checkList.AddRange(new string[] { contractList[1], contractList[3] });
				CheckValueParameter(checkList);
			}
			else if ((contractList.Count < 4) && (property.GetGetMethod(true) != null) && (DbcAccessType != AccessType.OnlyOnSet))
			{
				checkList.Add( contractList[0] );
				CheckValueParameter(checkList);
			}
		}

		/// <summary>
		/// Prüft die richtige Verwendung des Result Parameters bei Methoden und Eigenschaften.
		/// </summary>
		/// <param name="requireContractList">Alle Kontrakte die geprüft werden sollen.</param>
		private void CheckResultParameter(List<string> contractList)
		{
			// Der Apekt-Parameter '[result]' kann nicht in Eintrittskontrakte definiert werden da 
			// es zu diesem Zeitpunkt keinen alten Wert gibt.
			foreach (string contract in contractList)
			{
				// Der Apekt-Parameter '[result]' kann nicht in Eintrittskontrakte definiert werden da 
				// es zu diesem Zeitpunkt keinen alten Wert gibt.
				if ((!string.IsNullOrEmpty(contract)) && (contract.IndexOf(Resources.StrAspectParameterResultAccess) >= 0))
					throw new ContractCompilerExcpetion(string.Format(Resources.ExcResultAccesNotValidOnMethodAndProperty, Resources.StrAspectParameterResultAccess));
			}
		}

		/// <summary>
		/// Prüft die richtige Verwendung des value Parameters bei Methoden und Eigenschaften.
		/// </summary>
		/// <param name="requireContractList">Alle Kontrakte die geprüft werden sollen.</param>
		private void CheckValueParameter(List<string> contractList)
		{
			// Der Apekt-Parameter '[value]' kann nicht in Kontrakten verwendet werden die
			// an Methoden oder bei Lesezugriff (get) von Eigenschaften, definiert sind.
			foreach (string contract in contractList)
			{
				// Der Apekt-Parameter '[value]' kann nicht in Kontrakten verwendet werden die
				// an Methoden oder bei Lesezugriff (get) von Eigenschaften, definiert sind.
				if ((!string.IsNullOrEmpty(contract)) && (contract.IndexOf(Resources.StrAspectParameterValueAccess) >= 0))
					throw new ContractCompilerExcpetion(string.Format(Resources.ExcValueAccesNotValidOnMethodAndProperty, Resources.StrAspectParameterValueAccess));
			}
		}

		/// <summary>
		/// Generiert ein CompilerFehler.
		/// </summary>
		/// <param name="error">Fehlertext</param>
		/// <param name="element">Element an dem der Fehlerhafte Kontrakt definiert wurde.</param>
		private void CreateCompilerError(string error, MemberInfo element)
		{
			string elementName = string.Empty;
			string assemblyName = string.Empty;
			string namespaceName = string.Empty;
			string className = string.Empty;
			if (element != null)
			{
				if (element != null)
				{
					elementName = element.Name;
					assemblyName = element.DeclaringType.Assembly.FullName;
					namespaceName = element.DeclaringType.Namespace;
					className = element.DeclaringType.Name;
				}
			}
			DbcMessageSource.Instance.Write(SeverityType.Error, "ExcCompileError",
				new Object[] { elementName, assemblyName, namespaceName, className, error });
		}

		/// <summary>
		/// Gibt eine Liste mit Kontrakten zurück die bei Eintritt geprüft werden.
		/// </summary>
		/// <param name="contractList">Liste mit allen Konrakten.</param>
		/// <returns>Liste mit Eintrittskontrakten</returns>
		private List<string> GetRequireContracts(List<string> contractList)
		{
			List<string> requireContracts = new List<string>();
			if (contractList.Count == 4)
			{
				requireContracts.Add(contractList[0]);
				requireContracts.Add(contractList[1]);
			}
			else if (contractList.Count == 2)
				requireContracts.Add(contractList[0]);
			else if ((contractList.Count == 1) && (DbcCheckTime != CheckTime.OnlyEnsure))
				requireContracts.Add(contractList[0]);
			return requireContracts;
		}

		/// <summary>
		/// Ordnet dem Elementtyp den passenden Aspekt zu.
		/// </summary>
		/// <param name="element">Element für welches ein Kontrakt vorliegt.</param>
		/// <param name="collection">Liste mit den Aspekten</param>
		public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
		{
			if (!(element is MemberInfo))
			{
				CreateCompilerError(string.Format(Resources.ExcElementisNoMemberInfo, new object[]{element}), null);
				return;
			}
			try
			{
				// Hier werden alle Elemente übergeben, die einen Kontrakt definieren,
				// für Eigenschaften und dessen get bzw. set Methode erfolgt einen Aufruf
				// dieser Redundante Aufruf wird hier verhindert.
				if ((!AspectController.Instance.CanAttachAspectToMember((MemberInfo)element))
					|| ((element is MethodInfo) && (((MethodInfo)element).IsSpecialName)))
					return;
				CheckContracts((MemberInfo)element, mContracts);

				if (element is PropertyInfo)
					AddAspectToProperty(mContracts, collection, (PropertyInfo)element);
				else if (element is MethodInfo)
					AddAspectToMethod(mContracts, collection, (MethodInfo)element);
				else if (element is FieldInfo)
					AddAspectToField(mContracts, collection, (FieldInfo)element);

			}
			catch(Exception exception)
			{
				CreateCompilerError(exception.ToString(), (MemberInfo)element);
			}
		}

		/// <summary>
		/// Liefert die Anzahl der verschieden definierten Kontrakte des Aspects zurück.
		/// </summary>
		/// <returns>Anzahl der verschieden definierten Kontrakte.</returns>
		internal int GetNumberOfContracts()
		{
			// Der Apekt wurde an eine Eigenschaft definiert
			if (mContracts.Count == 4)
				return (GetNumberOfMemberContracts(mContracts[0], mContracts[1]) + GetNumberOfMemberContracts(mContracts[2], mContracts[3]));
			// Der Apekt wurde an eine Eigenschaft definiert
			else if (mContracts.Count == 2)
				return GetNumberOfMemberContracts(mContracts[0], mContracts[1]);
			else if (mContracts.Count == 1)
				return 1;
			return 0;
		}

		/// <summary>
		/// Gibt die Anzahl der verschieden definierten Kontrakte für ein Element zurück.
		/// </summary>
		/// <param name="firstContract">Der erste Kontrakt des Elements.</param>
		/// <param name="secondContract">Der zweite Kontrakt des Elements.</param>
		/// <returns>Die Anzahl der verschieden definierten Kontrakte.</returns>
		private int GetNumberOfMemberContracts(string firstContract, string secondContract)
		{
			// Wenn keine Konrakte definiert sind.
			if ((string.IsNullOrEmpty(firstContract))
				&& (string.IsNullOrEmpty(secondContract)))
				return 0;
			// Wenn nur ein Kontrakt definiert ist.
			if (((string.IsNullOrEmpty(firstContract))
				|| (string.IsNullOrEmpty(secondContract)))
				&& (firstContract != secondContract))
				return 1;
			// Wenn zwei Kontrakte definiert sind.
			if ((!(string.IsNullOrEmpty(firstContract)))
				&& (!(string.IsNullOrEmpty(secondContract)))
				&& (firstContract != secondContract))
				return 2;
			return 0;
		}


		#endregion Methoden 

	}
}
