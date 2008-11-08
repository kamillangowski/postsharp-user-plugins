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
using System.Reflection;
using Aspect.DesignByContract.MessageSources;
using PostSharp.Extensibility;
using Aspect.DesignByContract.Controller;
using Aspect.DesignByContract.Enums;
using System.Runtime.Serialization;

namespace Aspect.DesignByContract.SubAspects
{
	/// <summary>
 	/// Aspekt für Ein bzw. Austrittsprüfungen auf Methoden.
	/// </summary>
	[Serializable]
    internal class MethodBoundaryAspect : OnMethodBoundaryAspect, IDeserializationCallback
	{
        // BUG
        // Bitte wieder entfernen dirty Hack
        private bool mStarted = false;
        private string mAssemblyName = string.Empty;

		#region Interne Variablen (2) 

		/// <summary>
		/// Für die Methode gültiges KontraktModel.
		/// </summary>
		private MethodModel mContractModel = null;
		/// <summary>
		/// Speicher bei Methodeneintritt die Werte die bei Austritt geprüft werden sollen.
		/// </summary>
		[NonSerialized]
		private object[] mOldValues = null;

		#endregion Interne Variablen 

		#region Events (2) 

		/// <summary>
		/// Methode wird bei Aufruf einer Methode aufgerufen an der ein Kontrakt definiert wurde.
		/// </summary>
		/// <param name="eventArgs">FieldAccessEventArgs für die Methode die aufgerufen wurde.</param>
		public override void OnEntry(MethodExecutionEventArgs eventArgs)
		{
            // Bug
            // Hacking Code bitte entfernen
            Initialize(eventArgs.Method.DeclaringType.Assembly);

            // Die Typen die zur Designzeit als generisch geladen wurden müssen
			// zur Laufzeit als konkrete Typen definiert werden. Diese konktreten
			// Typen laden. Es müssen die Typen immer geladen werden, da es sonst
			// bei verschiedene instanzierungen um verschiedene Typen handeln kann.
			if ((!mContractModel.GenericTypesLoaded)
				&& (!eventArgs.Instance.GetType().IsGenericType))
				mContractModel.GenericTypesLoaded = true;
			if (!mContractModel.GenericTypesLoaded)
				mContractModel.GenericClassTypes = eventArgs.Instance.GetType().GetGenericArguments();

			// Wenn das IContract Objekt noch nicht geladen wurde bitte laden
			if (mContractModel.ContractObject == null)
				mContractModel.ContractObject = ContractController.Instance.GetContractObject(mContractModel.ContractClassName);

			// als alt definierten Werte laden um sie beim Austritt vergleichen zu können.
			if (mContractModel.EnsureContract.OldValueExist)
				mOldValues = mContractModel.ContractObject.GetOldValues(mContractModel.EnsureContract.GetOldValueKey, eventArgs.GetArguments(), eventArgs.Instance, mContractModel.GenericClassTypes);

			if (mContractModel.DbcCheckTime == CheckTime.OnlyEnsure)
				return;

			// Kontraktprüfung
			mContractModel.ContractObject.CheckContract(
				mContractModel.RequireContract.ContractKey,
				eventArgs.GetArguments(),
				eventArgs.Instance,
				eventArgs.ReturnValue,
				mContractModel.GenericClassTypes,
				null);


			base.OnEntry(eventArgs);
		}

		/// <summary>
		/// Methode wird nach erfolgreiche Abarbeiten der Methodenlogik an der ein Kontrakt definiert wurde aufgerufen.
		/// </summary>
		/// <param name="eventArgs">FieldAccessEventArgs für die Methode die aufgerufen wurde.</param>
		public override void OnSuccess(MethodExecutionEventArgs eventArgs)
		{

			if (mContractModel.DbcCheckTime == CheckTime.OnlyRequire)
				return;

			// Kontraktprüfung
			mContractModel.ContractObject.CheckContract(
				mContractModel.EnsureContract.ContractKey,
				eventArgs.GetArguments(),
				eventArgs.Instance,
				eventArgs.ReturnValue,
				mContractModel.GenericClassTypes,
				mOldValues);

			base.OnSuccess(eventArgs);
		}

		#endregion Events 

		#region Kon/Destructoren (Dispose) (1) 

		/// <summary>
		/// Konsturktor
		/// </summary>
		/// <param name="contactModel">Kontraktinformationen für die Methode</param>
		internal MethodBoundaryAspect(MethodModel contactModel)
		{
			mContractModel = contactModel;
		}

		#endregion Kon/Destructoren (Dispose) 

		#region Methoden (3) 

		/// <summary>
		/// Wird zur Kompilierzeit aufgerufen.
		/// </summary>
		/// <param name="method">Methode an der der Aspekt definiert wurde.</param>
		public override void CompileTimeInitialize(MethodBase method)
		{
			try
			{
                // Kontrakte an abstrakten Klassen oder Schnittstellen müssen nicht erzeugt werden.
                if ((method.DeclaringType.IsAbstract)
                    || (method.DeclaringType.IsInterface))
                    return;

                // Bug
                // Hack bitte entfernen
                mAssemblyName = method.DeclaringType.Assembly.FullName;

				ExpressionController expressionController = new ExpressionController();
				ExpressionModel expressionModel = null;

                //System.Windows.Forms.MessageBox.Show(mContractModel.RequireContract.Contract + "\n\r" + mContractModel.EnsureContract.Contract);
                if ((mContractModel.RequireContract != null)
					&& (!(string.IsNullOrEmpty(mContractModel.RequireContract.Contract))))
				{
					// Ausdruck übersetzen
					expressionModel = expressionController.ConvertExpression(mContractModel.RequireContract.Contract, mContractModel.Member);
					mContractModel.RequireContract.ConvertedContract = expressionModel.ConvertedExpression;
					mContractModel.RequireContract.GetOldValuesStatements = expressionModel.GetOldValueExpressions;
					// Code erzeugen lassen
					CodeController.Instance.AddContract(mContractModel.RequireContract, mContractModel);
				}
				if ((!mContractModel.ContractsAreEqual)
					&& (mContractModel.EnsureContract != null)
					&& (!(string.IsNullOrEmpty(mContractModel.EnsureContract.Contract))))
				{
					// Ausdruck übersetzen
					expressionModel = expressionController.ConvertExpression(mContractModel.EnsureContract.Contract, mContractModel.Member);
					mContractModel.EnsureContract.ConvertedContract = expressionModel.ConvertedExpression;
					mContractModel.EnsureContract.GetOldValuesStatements = expressionModel.GetOldValueExpressions;
					// Code erzeugen lassen
                    CodeController.Instance.AddContract(mContractModel.EnsureContract, mContractModel);
				}

				base.CompileTimeInitialize(method);
			}
			catch (Exception exception)
			{
				CreateCompilerError(exception.ToString(), method);
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

        private void Initialize(Assembly contractedAssembly)
        {
            if (mStarted)
                return;
            mStarted = true;
            if (mContractModel.ContractAssembly == null)
                return;

#if (DEBUG)
            ContractController.Instance.LoadContractAssembly(
                contractedAssembly,
                mContractModel.ContractClassName,
                mContractModel.ContractAssembly,
                mContractModel.PdbFile,
                mContractModel.SourceCodeFile);
#else
			ContractController.Instance.LoadContractAssembly(
				mContractModel.ContractClassName,
				mContractModel.ContractAssembly);
#endif
        }

		/// <summary>
		/// Wird aufgerufen wenn zur Laufzeit auf einen Aspekt initial zugegriffen wird = daraufhin 
		/// werden alle Aspekte initalisiert.
		/// </summary>
		/// <param name="field">Methoden Element</param>
		public override void RuntimeInitialize(MethodBase method)
		{
            Initialize(method.DeclaringType.Assembly);
            return;
//            if (mContractModel.ContractAssembly == null)
//                return;
//#if (DEBUG)
//            ContractController.Instance.LoadContractAssembly(
//                method.DeclaringType.Assembly,
//                mContractModel.ContractClassName,
//                mContractModel.ContractAssembly,
//                mContractModel.PdbFile,
//                mContractModel.SourceCodeFile);
//#else
//            ContractController.Instance.LoadContractAssembly(
//                mContractModel.ContractClassName,
//                mContractModel.ContractAssembly);
//#endif

//            base.RuntimeInitialize(method);
		}

		#endregion Methoden 

	
        #region IDeserializationCallback Member

        /// <summary>
        /// Bug...
        /// </summary>
        /// <param name="sender"></param>
        public void OnDeserialization(object sender)
        {
#if (DEBUG)
   			Assembly[] appAssemblies = AppDomain.CurrentDomain.GetAssemblies();

			for (int i = 0; i < appAssemblies.Length; i++)
            {
                if (appAssemblies[i].FullName == mAssemblyName)
                    Initialize(appAssemblies[i]);
            }
#else
            Initialize(null);
#endif
        }

        #endregion
    }
}
