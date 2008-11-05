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
using Aspect.DesignByContract.Models;
using PostSharp.Laos;
using Aspect.DesignByContract.MessageSources;
using System.Reflection;
using PostSharp.Extensibility;
using Aspect.DesignByContract.Controller;
using Aspect.DesignByContract.Enums;

namespace Aspect.DesignByContract.SubAspects
{
	/// <summary>
	/// Aspekt für Lese bzw. Schreibzugriffe auf Felder
	/// </summary>
	[Serializable]
	internal class FieldAccessAspect : OnFieldAccessAspect
	{

		#region Interne Variablen (1) 
        // BUG
        // Bitte wieder entfernen dirty Hack
        private bool mStarted = false;

		/// <summary>
		/// Für die Methode gültiges KontraktModel.
		/// </summary>
		private FieldModel mFieldModel = null;

		#endregion Interne Variablen 

		#region Events (2) 

		/// <summary>
		/// Methode wird aufgerufen wenn es ein Lesezugriff auf ein Feld gibt.
		/// </summary>
		/// <param name="eventArgs">FieldAccessEventArgs für das Feld auf das ein Lesezugriff ausgeführt wird.</param>
		public override void OnGetValue(FieldAccessEventArgs eventArgs)
		{
            Initialize(eventArgs.FieldInfo);
			if (mFieldModel.DbcAccessType == AccessType.OnlyOnSet)
			{
				base.OnGetValue(eventArgs);
				return;
			}

			// Die Typen die zur Designzeit als generisch geladen wurden müssen
			// zur Laufzeit als konkrete Typen definiert werden. Diese konktreten
			// Typen laden. Es müssen die Typen immer geladen werden, da es sonst
			// bei verschiedene instanzierungen um verschiedene Typen handeln kann.
			if ((!mFieldModel.GenericTypesLoaded)
				&& (!eventArgs.Instance.GetType().IsGenericType))
				mFieldModel.GenericTypesLoaded = true;
			if (!mFieldModel.GenericTypesLoaded)
				mFieldModel.GenericClassTypes = eventArgs.Instance.GetType().GetGenericArguments();


			// Wenn das IContract Objekt noch nicht geladen wurde bitte laden
			if (mFieldModel.ContractObject == null)
				mFieldModel.ContractObject = ContractController.Instance.GetContractObject(mFieldModel.ContractClassName);

			// Kontraktprüfung
			mFieldModel.ContractObject.CheckContract(
				mFieldModel.GetContract.ContractKey,
				null,
				eventArgs.Instance,
				eventArgs.StoredFieldValue,
				mFieldModel.GenericClassTypes,
				null);

			base.OnGetValue(eventArgs);
		}

		/// <summary>
		/// Methode wird aufgerufen wenn es ein Schreibzugriff auf ein Feld gibt.
		/// </summary>
		/// <param name="eventArgs">FieldAccessEventArgs für das Feld auf das ein Schreibzugriff ausgeführt wird.</param>
		public override void OnSetValue(FieldAccessEventArgs eventArgs)
		{
            Initialize(eventArgs.FieldInfo);
			if (mFieldModel.DbcAccessType == AccessType.OnlyOnGet)
			{
				base.OnSetValue(eventArgs);
				return;
			}

			// Die Typen die zur Designzeit als generisch geladen wurden müssen
			// zur Laufzeit als konkrete Typen definiert werden. Diese konktreten
			// Typen laden. Es müssen die Typen immer geladen werden, da es sonst
			// bei verschiedene instanzierungen um verschiedene Typen handeln kann.
			if ((!mFieldModel.GenericTypesLoaded)
				&& (!eventArgs.Instance.GetType().IsGenericType))
				mFieldModel.GenericTypesLoaded = true;
			if (!mFieldModel.GenericTypesLoaded)
				mFieldModel.GenericClassTypes = eventArgs.Instance.GetType().GetGenericArguments();

			// Wenn das IContract Objekt noch nicht geladen wurde bitte laden
			if (mFieldModel.ContractObject == null)
				mFieldModel.ContractObject = ContractController.Instance.GetContractObject(mFieldModel.ContractClassName);

			// Speichern der Werte die in das Feld geschrieben wurden.
			object[] fieldValue = new object[] { eventArgs.ExposedFieldValue };

			// Kontraktprüfung
			mFieldModel.ContractObject.CheckContract(
				mFieldModel.SetContract.ContractKey,
				fieldValue,
				eventArgs.Instance,
				eventArgs.StoredFieldValue,
				mFieldModel.GenericClassTypes,
				null);

			base.OnSetValue(eventArgs);
		}

		#endregion Events 

		#region Kon/Destructoren (Dispose) (1) 

		/// <summary>
		/// Konsturktor
		/// </summary>
		/// <param name="contactModel">Kontraktinformationen für die Methode</param>
		internal FieldAccessAspect(FieldModel fieldModel)
		{
			mFieldModel = fieldModel;
		}

		#endregion Kon/Destructoren (Dispose) 

		#region Methoden (3) 

		/// <summary>
		/// Wird zur Kompilierzeit aufgerufen.
		/// </summary>
		/// <param name="field">Feld an dem der Aspekt definiert wurde.</param>
		public override void CompileTimeInitialize(System.Reflection.FieldInfo field)
		{
			try
			{
				ExpressionController expressionController = new ExpressionController();
				ExpressionModel expressionModel = null;

				if ((mFieldModel.GetContract != null)
					&& (!(string.IsNullOrEmpty(mFieldModel.GetContract.Contract))))
				{
					// Ausdruck übersetzen
					expressionModel = expressionController.ConvertExpression(mFieldModel.GetContract.Contract, mFieldModel.Member);
					mFieldModel.GetContract.ConvertedContract = expressionModel.ConvertedExpression;
					mFieldModel.GetContract.GetOldValuesStatements = expressionModel.GetOldValueExpressions;
					// Code erzeugen lassen
					CodeController.Instance.AddContract(mFieldModel.GetContract, mFieldModel);
				}
				if ((!mFieldModel.ContractsAreEqual)
					&& (mFieldModel.SetContract != null)
					&& (!(string.IsNullOrEmpty(mFieldModel.SetContract.Contract))))
				{
					// Ausdruck übersetzen
					expressionModel = expressionController.ConvertExpression(mFieldModel.SetContract.Contract, mFieldModel.Member);
					mFieldModel.SetContract.ConvertedContract = expressionModel.ConvertedExpression;
					mFieldModel.SetContract.GetOldValuesStatements = expressionModel.GetOldValueExpressions;
					// Code erzeugen lassen
					CodeController.Instance.AddContract(mFieldModel.SetContract, mFieldModel);
				}

				base.CompileTimeInitialize(field);
			}
			catch (Exception exception)
			{
				CreateCompilerError(exception.ToString(), field);
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

        private void Initialize(FieldInfo field)
        {
            if (mStarted)
                return;
            mStarted = true;

            // Die Typen die zur Designzeit als generisch geladen wurden müssen
            // zur Laufzeit als konkrete Typen definiert werden. Diese konktreten
            // Typen laden.
            if (mFieldModel.GenericClassTypes == null)
                mFieldModel.GenericClassTypes = field.DeclaringType.GetGenericArguments();


            if (mFieldModel.ContractAssembly == null)
                return;
#if (DEBUG)
            ContractController.Instance.LoadContractAssembly(
                field.DeclaringType.Assembly,
                mFieldModel.ContractClassName,
                mFieldModel.ContractAssembly,
                mFieldModel.PdbFile,
                mFieldModel.SourceCodeFile);
#else
			ContractController.Instance.LoadContractAssembly(
				mFieldModel.ContractClassName,
				mFieldModel.ContractAssembly);
#endif
        }

		/// <summary>
		/// Wird aufgerufen wenn zur Laufzeit auf einen Aspekt initial zugegriffen wird = daraufhin 
		/// werden alle Aspekte initalisiert.
		/// </summary>
		/// <param name="field">Feld Element</param>
		public override void RuntimeInitialize(FieldInfo field)
		{
            return;
//            // Die Typen die zur Designzeit als generisch geladen wurden müssen
//            // zur Laufzeit als konkrete Typen definiert werden. Diese konktreten
//            // Typen laden.
//            if (mFieldModel.GenericClassTypes == null)
//                mFieldModel.GenericClassTypes = field.DeclaringType.GetGenericArguments();


//            if (mFieldModel.ContractAssembly == null)
//                return;
//#if (DEBUG)
//            ContractController.Instance.LoadContractAssembly(
//                field.DeclaringType.Assembly,
//                mFieldModel.ContractClassName,
//                mFieldModel.ContractAssembly,
//                mFieldModel.PdbFile,
//                mFieldModel.SourceCodeFile);
//#else
//            ContractController.Instance.LoadContractAssembly(
//                mFieldModel.ContractClassName,
//                mFieldModel.ContractAssembly);
//#endif

//            base.RuntimeInitialize(field);
        }

        #endregion Methoden 

	}
}
