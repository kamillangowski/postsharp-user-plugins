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
using System.CodeDom;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using Aspect.DesignByContract.Models;
using Aspect.DesignByContract.Properties;
using Aspect.DesignByContract.Enums;
using Aspect.DesignByContract.Interface;
using Aspect.DesignByContract.Exceptions;
using PostSharp.Extensibility;

namespace Aspect.DesignByContract.Controller
{
	/// <summary>
	/// Singleton der den Source Code erzeugt.
	/// </summary>
	internal class CodeController
	{

		#region Interne Variablen (10) 

		/// <summary>
		/// Das Model welches die Daten der ContractAssembly beinhaltet
		/// </summary>
		private CodeCompileUnit mAssemblyCompileUnit = null;
		/// <summary>
		/// Die CheckContract Methode die in der IContract Schnittstelle definiert ist.
		/// </summary>
		private CodeMemberMethod mCheckContractMethodModel = null;
		/// <summary>
		/// Die Klasse die erzeugt wird und alle Kontraktprüfungen beinhaltet.
		/// </summary>
		private CodeTypeDeclaration mClassModel = null;
		/// <summary>
		/// Membervariable der Eigenschaft DbcAspectCount.
		/// </summary>
		private int mDbcAspectCount = int.MinValue;
		/// <summary>
		/// Membervariable der Eigenschaft DbcAspectCurrentCount.
		/// </summary>
		private int mDbcAspectCurrentCount = int.MinValue;
		/// <summary>
		/// Die GetOldParameter Methode die in der IContract Schnittstelle definiert ist.
		/// </summary>
		private CodeMemberMethod mGetOldValuesMethodModel = null;
		/// <summary>
		/// Singletonmember
		/// </summary>
		private static CodeController mInstance = null;
		/// <summary>
		/// Zum Locken damit nicht 2 mal eine Instanz von der Klasse
		/// ConractGeneratorr erzeugt wird.
		/// </summary>
		private static object mLockObject = new object();
		/// <summary>
		/// Membervariable der Eigenschaft SourceCodeGenerated.
		/// </summary>
		private bool mSourceCodeGenerated = false;
		/// <summary>
		/// Membervariable der Eigenschaft Started.
		/// </summary>
		private bool mStarted = false;

		#endregion Interne Variablen 

		#region Eigenschaften (5) 

		/// <summary>
		/// Anzahl der Aspekte in der aktuellen Assembly.
		/// </summary>
		internal int DbcAspectCount
		{
			get { return mDbcAspectCount; }
			private set { mDbcAspectCount = value; }
		}

		/// <summary>
		/// Gibt an der wievielte Aspekt gerade bearbeitet wird.
		/// </summary>
		internal int DbcAspectCurrentCount
		{
			get { return mDbcAspectCurrentCount; }
			private set { mDbcAspectCurrentCount = value; }
		}

		/// <summary>
		/// Singleton Eigenschaft
		/// </summary>
		internal static CodeController Instance
		{
			get
			{
				if (mInstance != null)
					return mInstance;
				lock (mLockObject)
				{
					if (mInstance == null)
						mInstance = new CodeController();
					return mInstance;
				}
			}
		}

		/// <summary>
		/// Gibt an, ob der Sourcecode erzeugt wurde.
		/// </summary>
		internal bool SourceCodeGenerated
		{
			get { return mSourceCodeGenerated; }
			private set { mSourceCodeGenerated = value; }
		}

		/// <summary>
		/// Gibt zurück ob der ContractGenerator gestartet wurde.
		/// </summary>
		internal bool Started
		{
			get { return mStarted; }
		}

		#endregion Eigenschaften 

		#region Kon/Destructoren (Dispose) (1) 

		/// <summary>
		/// Konstruktor
		/// </summary>
		private CodeController()
		{
		}

		#endregion Kon/Destructoren (Dispose) 

		#region Methoden (10) 

		/// <summary>
		/// Fügt eine weiter Kontraktprüfung der zu erzeugenden Klasse bzw. Assembly hinzu.
		/// </summary>
		/// <param name="contractModel">Modell mit allen relevanten Daten zum Kontrakt</param>
		/// <param name="aspectModel">Modell mit allen relevanten Daten zum Aspekt der den Kontrakt prüft</param>
		/// <returns>Sofern es sich um den letzten Aspekt handelt</returns>
		internal void AddContract(ContractModel contractModel, MemberBaseModel aspectModel)
		{
			if (!Started)
				StartContractGenerator(aspectModel.ContractClassName, aspectModel.Member.DeclaringType.Assembly);

			DbcAspectCurrentCount++;

			// Den Contract in einen gültigen Ausdruck übersetzen
			//ExpressionController exprController = new ExpressionController();
			//ExpressionModel exprModel = exprController.ConvertExpression(contractModel, aspectModel.Member);

			CodeMemberMethod contractMethod = AddContractMethod(contractModel, aspectModel.Member, aspectModel.ExceptionType, aspectModel.ExceptionString);
			mClassModel.Members.Add(contractMethod);
			//Methodenaufruf in der CheckContract Methode erzeugen.
			CodeConditionStatement contractCondition = AddContractMethodCall(contractModel.ContractKey, contractModel.OldValueExist);
			mCheckContractMethodModel.Statements.Add(contractCondition);

			if (contractModel.OldValueExist)
			{
			    CodeMemberMethod getOldValueMethod = AddGetOldValueMethod(contractModel, aspectModel.Member);
			    mClassModel.Members.Add(getOldValueMethod);
			    //Methodenaufruf in der GetOldValue Methode erzeugen.
			    CodeConditionStatement getOldValueCondition = AddGetOldValueMethodCall(contractModel.GetOldValueKey);
			    mGetOldValuesMethodModel.Statements.Add(getOldValueCondition);
			}

			// Da es sich um den letzten Kontrakt handelt die Assemlby erzeugen und
			// in das ContractModel speichern.
			if ((DbcAspectCount == DbcAspectCurrentCount)
				&& (!SourceCodeGenerated))
				//System.Windows.Forms.MessageBox.Show("Erzeugen des Codes");
				GenerateCode(aspectModel);
		}

		/// <summary>
		/// Fügt eine weitere Kontrakt Methode der zu erzeugenden Klasse hinzu.
		/// </summary>
		/// <param name="contract">ContractModel mit allen nötigen Daten</param>
		/// <param name="contractedElement">Das Element an dem der Kontrakt definiert wurde.</param>
		/// <param name="exceptionType">Übergibt den Typ der Exception, der geworfen werden soll wenn der Kontrakt nicht erfüllt wird.</param>
		/// <param name="exceptionString">Übergibt den Text der Exception, der geworfen werden soll wenn der Kontrakt nicht erfüllt wird.</param>
		/// <returns>CodeMemberMethod Objekt mit der Kontraktprüfung.</returns>
		private CodeMemberMethod AddContractMethod(ContractModel contract, MemberInfo contractedElement, Type exceptionType, string exceptionString)
		{
			//Methode anlegen
			CodeMemberMethod contractMethod = new CodeMemberMethod();
			contractMethod.Name = contract.ContractKey;
			contractMethod.ReturnType = new CodeTypeReference(typeof(void));
			contractMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;

			// Übergabeparameter festlegen
			contractMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object[])), Resources.StrParameterContractArguments));
			contractMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object)), Resources.StrParameterInstance));
			contractMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object)), Resources.StrParameterMethodResult));
			contractMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Type[])), Resources.StrParameterGenericTypes));
			if (contract.OldValueExist)
				contractMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object[])), Resources.StrParameterOldValues));

			//if Statement erzeugen
			CodeConditionStatement contractStatement = new CodeConditionStatement();
			contractStatement.Condition = new CodeSnippetExpression(contract.ConvertedContract);

			//return hinzufügen
			contractStatement.TrueStatements.Add(new CodeMethodReturnStatement());

			//Exception erzeugen
			string fullMethodName = contractedElement.DeclaringType.Namespace + "." + contractedElement.DeclaringType.Name + "." + contractedElement.Name;
			if (exceptionType == null)
				exceptionType = typeof(ContractException);
			if (string.IsNullOrEmpty(exceptionString))
				exceptionString = string.Format(CultureInfo.CurrentCulture, Resources.ExcContractNotValid, new object[] { contract.Contract, fullMethodName });
			CodeThrowExceptionStatement throwContractException = new CodeThrowExceptionStatement(new CodeObjectCreateExpression(new CodeTypeReference(exceptionType), new CodeExpression[] {new CodePrimitiveExpression(exceptionString )}));

			// Try Catch um diese Methode legen
			CodeTryCatchFinallyStatement tryContrainer = new CodeTryCatchFinallyStatement();
			tryContrainer.TryStatements.Add(contractStatement);

			// Catch Clauses erzeugen
			CodeCatchClause exceptionClause = new CodeCatchClause(Resources.StrParameterException);
			exceptionClause.CatchExceptionType = new CodeTypeReference(typeof(Exception));
			exceptionClause.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(new CodeTypeReference(typeof(ContractException)), new CodeExpression[] { new CodePrimitiveExpression(string.Format(CultureInfo.CurrentCulture, Resources.ExcErrorInContract, new object[] { contract.Contract, fullMethodName })), new CodeVariableReferenceExpression(Resources.StrParameterException) })));

			tryContrainer.CatchClauses.Add(exceptionClause);

			// Den kompletten Inhalt der Methode zuordnen
			contractMethod.Statements.Add(tryContrainer);
			contractMethod.Statements.Add(throwContractException);

			return contractMethod;
		}

		/// <summary>
		/// Erzeugt eine if-Abfrage um die korrekte Methode die die 
		/// Kontraktabfrage beinhaltet aufzurufen.
		/// </summary>
		/// <param name="contractMethodName">Name der Methode die aufgerufen werden soll.</param>
		/// <param name="oldValueAccess">Gibt an ob der Kontrakt die OldValues benötigt.</param>
		/// <returns>Das Code Condition Objekt welches in die CheckContract 
		/// Methode der IContract Schnittstelle eingebunden werden soll.</returns>
		private CodeConditionStatement AddContractMethodCall(string methodName, bool oldValueAccess)
		{
			// if Statement erzeugen
			CodeConditionStatement ifStatement = new CodeConditionStatement();
			CodeBinaryOperatorExpression binaryOperator = new CodeBinaryOperatorExpression();
			CodeVariableReferenceExpression value1 = new CodeVariableReferenceExpression();
			value1.VariableName = Resources.StrParameterContractKey;
			binaryOperator.Left = value1;
			binaryOperator.Operator = CodeBinaryOperatorType.IdentityEquality;
			CodePrimitiveExpression value2 = new CodePrimitiveExpression();
			value2.Value = methodName;
			binaryOperator.Right = value2;
			ifStatement.Condition = binaryOperator;

			// Methodenaufruf erzeugen
			CodeExpressionStatement excpressionStatement = new CodeExpressionStatement();
			CodeMethodInvokeExpression invokeExpression = new CodeMethodInvokeExpression();

			// Übergabeparameter contractArguments erzeugen
			CodeVariableReferenceExpression referenceContractArguments = new CodeVariableReferenceExpression();
			referenceContractArguments.VariableName = Resources.StrParameterContractArguments;
			invokeExpression.Parameters.Add(referenceContractArguments);

			// Übergabeparameter instance erzeugen
			CodeVariableReferenceExpression referenceInstance = new CodeVariableReferenceExpression();
			referenceInstance.VariableName = Resources.StrParameterInstance;
			invokeExpression.Parameters.Add(referenceInstance);

			// Übergabeparameter methodResult erzeugen
			CodeVariableReferenceExpression methodResult = new CodeVariableReferenceExpression();
			methodResult.VariableName = Resources.StrParameterMethodResult;
			invokeExpression.Parameters.Add(methodResult);

			// Übergabeparameter genericTypes erzeugen
			CodeVariableReferenceExpression referenceGenericType = new CodeVariableReferenceExpression();
			referenceGenericType.VariableName = Resources.StrParameterGenericTypes;
			invokeExpression.Parameters.Add(referenceGenericType);

			if (oldValueAccess)
			{
				CodeVariableReferenceExpression oldValues = new CodeVariableReferenceExpression();
				oldValues.VariableName = Resources.StrParameterOldValues;
				invokeExpression.Parameters.Add(oldValues);
			}

			// Methodenaufruf zusammenfügen
			CodeMethodReferenceExpression aContractMethodExpression = new CodeMethodReferenceExpression();
			aContractMethodExpression.MethodName = methodName;
			CodeThisReferenceExpression aThisRefExpression = new CodeThisReferenceExpression();
			aContractMethodExpression.TargetObject = aThisRefExpression;
			invokeExpression.Method = aContractMethodExpression;
			excpressionStatement.Expression = invokeExpression;
			ifStatement.TrueStatements.Add(excpressionStatement);
			// return hinzufügen
			ifStatement.TrueStatements.Add(new CodeMethodReturnStatement());

			return ifStatement;
		}

		/// <summary>
		/// Erzeugt eine Element basierte GetOldValue Methode um die als alten Werte gekennzeichneten 
		/// Werte für einen Kontakt zu laden.
		/// </summary>
		/// <param name="contract">ContractModel mit allen nötigen Daten</param>
		/// <param name="contractedElement">Das Element an dem der Kontrakt definiert wurde.</param>
		/// <returns>CodeMemberMethod Objekt mit der dem OldValues als Rückgabewerte.</returns>
		private CodeMemberMethod AddGetOldValueMethod(ContractModel contract, MemberInfo contractedElement)
		{
			//Methode anlegen
			CodeMemberMethod getOldValueMethod = new CodeMemberMethod();
			getOldValueMethod.Name = contract.GetOldValueKey;
			getOldValueMethod.ReturnType = new CodeTypeReference(typeof(object[]));
			getOldValueMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;

			// Übergabeparameter festlegen
			getOldValueMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object[])), Resources.StrParameterContractArguments));
			getOldValueMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object)), Resources.StrParameterInstance));
			getOldValueMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Type[])), Resources.StrParameterGenericTypes));

			// Try Catch um diese Methode legen
			CodeTryCatchFinallyStatement tryContrainer = new CodeTryCatchFinallyStatement();
			foreach (string expression in contract.GetOldValuesStatements)
				tryContrainer.TryStatements.Add(new CodeSnippetExpression(expression));

			string fullMethodName = contractedElement.DeclaringType.Namespace + "." + contractedElement.DeclaringType.Name + "." + contractedElement.Name;
			// Catch Clauses erzeugen
			CodeCatchClause exceptionClause = new CodeCatchClause(Resources.StrParameterException);
			exceptionClause.CatchExceptionType = new CodeTypeReference(typeof(Exception));
			exceptionClause.Statements.Add(new CodeThrowExceptionStatement(	new CodeObjectCreateExpression(new CodeTypeReference(typeof(ContractException)), new CodeExpression[] { new CodePrimitiveExpression(string.Format(CultureInfo.CurrentCulture, Resources.ExcErrorInContract, new object[] {contract.Contract, fullMethodName} ) ), new CodeVariableReferenceExpression(Resources.StrParameterException)})));

			tryContrainer.CatchClauses.Add(exceptionClause);

			// Den kompletten Inhalt der Methode zuordnen
			getOldValueMethod.Statements.Add(tryContrainer);

			return getOldValueMethod;
		}

		/// <summary>
		/// Erzeugt eine if-Abfrage um die als alten Werte gekennzeichneten Werte für einen Kontakt 
		/// zu laden.
		/// </summary>
		/// <param name="methodName">Name der Methode die aufgerufen werden soll.</param>
		/// <returns>Das Code Condition Objekt welches in die GetOldValue 
		/// Methode der IContract Schnittstelle eingebunden werden soll.</returns>
		private CodeConditionStatement AddGetOldValueMethodCall(string methodName)
		{
			// if Statement erzeugen
			CodeConditionStatement ifStatement = new CodeConditionStatement();
			CodeBinaryOperatorExpression binaryOperator = new CodeBinaryOperatorExpression();
			CodeVariableReferenceExpression value1 = new CodeVariableReferenceExpression();
			value1.VariableName = Resources.StrParameterContractKey;
			binaryOperator.Left = value1;
			binaryOperator.Operator = CodeBinaryOperatorType.IdentityEquality;
			CodePrimitiveExpression value2 = new CodePrimitiveExpression();
			value2.Value = methodName;
			binaryOperator.Right = value2;
			ifStatement.Condition = binaryOperator;

			// Methodenaufruf erzeugen
			CodeMethodInvokeExpression invokeExpression = new CodeMethodInvokeExpression();

			// Übergabeparameter contractArguments erzeugen
			CodeVariableReferenceExpression referenceContractArguments = new CodeVariableReferenceExpression();
			referenceContractArguments.VariableName = Resources.StrParameterContractArguments;
			invokeExpression.Parameters.Add(referenceContractArguments);

			// Übergabeparameter instance erzeugen
			CodeVariableReferenceExpression referenceInstance = new CodeVariableReferenceExpression();
			referenceInstance.VariableName = Resources.StrParameterInstance;
			invokeExpression.Parameters.Add(referenceInstance);

			// Übergabeparameter genericTypes erzeugen
			CodeVariableReferenceExpression referenceGenericType = new CodeVariableReferenceExpression();
			referenceGenericType.VariableName = Resources.StrParameterGenericTypes;
			invokeExpression.Parameters.Add(referenceGenericType);

			// Methodenaufruf zusammenfügen
			CodeMethodReferenceExpression aContractMethodExpression = new CodeMethodReferenceExpression();
			aContractMethodExpression.MethodName = methodName;
			CodeThisReferenceExpression aThisRefExpression = new CodeThisReferenceExpression();
			aContractMethodExpression.TargetObject = aThisRefExpression;
			invokeExpression.Method = aContractMethodExpression;
			// Der Methodenaufruf ist gleichzeitig auch ein return
			ifStatement.TrueStatements.Add(new CodeMethodReturnStatement(invokeExpression));

			return ifStatement;
		}

		/// <summary>
		/// Implementiert alle Methoden der IContract Schnittstelle 
		/// in die zu erzeugende Klasse.
		/// </summary>
		/// <param name="contractClass">Die Klasse in der die Methoden hinzugefügt werden sollen.</param>
		private void AddIContractInterface(CodeTypeDeclaration contractClass)
		{
			//Methode CheckContract hinzufügen die in der IContract Schnittstelle definiert wird
			mCheckContractMethodModel = new CodeMemberMethod();
			mCheckContractMethodModel.Name = Resources.StrCheckContractMethodName;
			mCheckContractMethodModel.Attributes = MemberAttributes.Public | MemberAttributes.Final;

			// Übergabeparameter festlegen
			mCheckContractMethodModel.Parameters.Add(new CodeParameterDeclarationExpression(
				new CodeTypeReference(typeof(string)), Resources.StrParameterContractKey));
			mCheckContractMethodModel.Parameters.Add(new CodeParameterDeclarationExpression(
				new CodeTypeReference(typeof(object[])), Resources.StrParameterContractArguments));
			mCheckContractMethodModel.Parameters.Add(new CodeParameterDeclarationExpression(
				new CodeTypeReference(typeof(object)), Resources.StrParameterInstance));
			mCheckContractMethodModel.Parameters.Add(new CodeParameterDeclarationExpression(
				new CodeTypeReference(typeof(object)), Resources.StrParameterMethodResult));
			mCheckContractMethodModel.Parameters.Add(new CodeParameterDeclarationExpression(
				new CodeTypeReference(typeof(Type[])), Resources.StrParameterGenericTypes));
			mCheckContractMethodModel.Parameters.Add(new CodeParameterDeclarationExpression(
				new CodeTypeReference(typeof(object[])), Resources.StrParameterOldValues));

			//Methode GetOldParameter hinzufügen die in der IContract Schnittstelle definiert wird
			mGetOldValuesMethodModel = new CodeMemberMethod();
			mGetOldValuesMethodModel.ReturnType = new CodeTypeReference(typeof(object[]));
			mGetOldValuesMethodModel.Name = Resources.StrGetOldPrarameterMethodName;
			mGetOldValuesMethodModel.Attributes = MemberAttributes.Public | MemberAttributes.Final;

			// Übergabeparameter festlegen
			mGetOldValuesMethodModel.Parameters.Add(new CodeParameterDeclarationExpression(
				new CodeTypeReference(typeof(string)), Resources.StrParameterContractKey));
			mGetOldValuesMethodModel.Parameters.Add(new CodeParameterDeclarationExpression(
				new CodeTypeReference(typeof(object[])), Resources.StrParameterContractArguments));
			mGetOldValuesMethodModel.Parameters.Add(new CodeParameterDeclarationExpression(
				new CodeTypeReference(typeof(object)), Resources.StrParameterInstance));
			mGetOldValuesMethodModel.Parameters.Add(new CodeParameterDeclarationExpression(
				new CodeTypeReference(typeof(Type[])), Resources.StrParameterGenericTypes));

			// Die Methoden der übergebenen Klasse zuordnen
			contractClass.Members.Add(mCheckContractMethodModel);
			contractClass.Members.Add(mGetOldValuesMethodModel);
		}

        private int GetMethodCount(Type type, bool searchForPostSharpAtt, bool abstractClass)
        {
            int attributeCount = 0;
            // Alle Methoden des Typs durchsuchen
            MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (MethodInfo methodInfo in methodInfos)
            {
                // BUG
                // das Attribut wird nicht umgesetzt daher muss nach HasInheritedAttributeAttribute.
                // Workaround
                if (searchForPostSharpAtt)
                {
                    if (methodInfo.GetCustomAttributes(typeof(HasInheritedAttributeAttribute), true).Length > 0)
                    {
                        if ((!abstractClass) || (methodInfo.IsAbstract))
                            attributeCount++;
                    }
                }
                else
                {
                    object[] methodAttributeObjects = methodInfo.GetCustomAttributes(typeof(Dbc), true);
                    foreach (object methodAttributeObject in methodAttributeObjects)
                    {
                        Dbc dbcAspect = (Dbc)methodAttributeObject;
                        attributeCount += dbcAspect.GetNumberOfContracts();
                    }
                }
            }
            return attributeCount;
        }

        private int GetPropertyCount(Type type, bool searchForPostSharpAtt)
        {
            int attributeCount=0;
            // Alle Eigenschaften des Typs durchsuchen
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                // BUG
                // das Attribut wird nicht umgesetzt daher muss nach HasInheritedAttributeAttribute.
                // Workaround
                if (searchForPostSharpAtt)
                {
                    if (propertyInfo.GetCustomAttributes(typeof(HasInheritedAttributeAttribute), true).Length > 0)
                        attributeCount++;
                }
                else
                {
                    object[] propertyAttributeObjects = propertyInfo.GetCustomAttributes(typeof(Dbc), true);
                    foreach (object propertyAttributeObject in propertyAttributeObjects)
                    {
                        Dbc dbcAspect = (Dbc)propertyAttributeObject;
                        attributeCount += dbcAspect.GetNumberOfContracts();
                    }
                }
            }
            return attributeCount;
        }

        private int GetFieldCount(Type type, bool searchForPostSharpAtt)
        {
            int attributeCount = 0;
            // Alle Felder des Typs durchsuchen
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                // BUG
                // das Attribut wird nicht umgesetzt daher muss nach HasInheritedAttributeAttribute.
                // Workaround
                if (searchForPostSharpAtt)
                {
                    if (fieldInfo.GetCustomAttributes(typeof(HasInheritedAttributeAttribute), true).Length > 0)
                        attributeCount++;
                }
                else
                {
                    object[] fieldAttributeObjects = fieldInfo.GetCustomAttributes(typeof(Dbc), true);
                    foreach (object fieldAttributeObject in fieldAttributeObjects)
                    {
                        Dbc dbcAspect = (Dbc)fieldAttributeObject;
                        attributeCount += dbcAspect.GetNumberOfContracts();
                    }
                }
            }
            return attributeCount;
        }

        private int GetConstructorCount(Type type, bool searchForPostSharpAtt, bool abstractClass)
        {
            int attributeCount = 0;
            // Alle Konstruktoren des Typs durchsuchen
            ConstructorInfo[] constructurInfos = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (ConstructorInfo constructurInfo in constructurInfos)
            {
                // BUG
                // das Attribut wird nicht umgesetzt daher muss nach HasInheritedAttributeAttribute.
                // Workaround
                if (searchForPostSharpAtt)
                {
                    if (constructurInfo.GetCustomAttributes(typeof(HasInheritedAttributeAttribute), true).Length > 0)
                    {
                        if ((!abstractClass) || (constructurInfo.IsAbstract))
                            attributeCount++;
                    }
                }
                else
                {
                    object[] constructorAttributeObjects = constructurInfo.GetCustomAttributes(typeof(Dbc), true);
                    foreach (object constructorAttributeObject in constructorAttributeObjects)
                    {
                        Dbc dbcAspect = (Dbc)constructorAttributeObject;
                        attributeCount += dbcAspect.GetNumberOfContracts();
                    }
                }
            }
            return attributeCount;
        }

        private int GetTypeCount(Type type, bool searchForPostSharpAtt, bool abstractClass)
        {
            int attributeCount = 0;

            attributeCount += GetConstructorCount(type, searchForPostSharpAtt, abstractClass);
            attributeCount += GetMethodCount(type, searchForPostSharpAtt, abstractClass);
            if (!abstractClass)
            {
                attributeCount += GetFieldCount(type, searchForPostSharpAtt);
                attributeCount += GetPropertyCount(type, searchForPostSharpAtt);
            }

            return attributeCount;
        }

		/// <summary>
		/// Ermittelt wie oft der DbcAspect in einer Assembly vorkommt.
		/// </summary>
		/// <param name="contractedAssembly">Die Assembly in der die Kontrakte definiert sind.</param>
		/// <returns>Anzahl der DbcAspekte in der aktuellen Assembly</returns>
		private int CountDbcAspectsInAssemblies(Assembly contractedAssembly)
		{
			int attributeCount = 0;

			// Alle Typen ermittlen
			Type[] assemblyTypes = contractedAssembly.GetTypes();
			foreach (Type assemblyType in assemblyTypes)
			{
                attributeCount += GetTypeCount(assemblyType, false, false);
                foreach (Type interfaceType in assemblyType.GetInterfaces())
                {
                    attributeCount += GetTypeCount(interfaceType, true, false);
                }
                Type baseType = assemblyType.BaseType;
                while (baseType != null)
                {
                    if (baseType.IsAbstract)
                        attributeCount += GetTypeCount(baseType, true, true);
                    baseType = baseType.BaseType;
                }
			}
            return attributeCount;
		}

		/// <summary>
		/// Beginnt den kompiliervorgang und erzeugt den Code und die Assembly.
		/// </summary>
		/// <param name="elementModel">Das Modell für das Element an dem ein Kontrakt definiert wurde.</param>
		private void GenerateCode(MemberBaseModel elementModel)
		{

			string sourceCodeFile = ContractController.Instance.CreateFileName(elementModel.Member.DeclaringType.Assembly, Resources.StrSourceCodeFielExt);
			string debugInformationFile = ContractController.Instance.CreateFileName(elementModel.Member.DeclaringType.Assembly, Resources.StrPdbFileExt);
			string contractAssemblyFile = ContractController.Instance.CreateFileName(elementModel.Member.DeclaringType.Assembly, Resources.StrAssemblyFileExt);


			// Der CheckContract Methode eine Exception werfen lassen wenn keine Kontraktmethode
			// gefunden wurde.
			mCheckContractMethodModel.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(new CodeTypeReference(typeof(ContractException)), new CodeExpression[] { new CodeBinaryOperatorExpression( new CodePrimitiveExpression(Resources.ExcCantFindMethodBegin), CodeBinaryOperatorType.Add, new CodeBinaryOperatorExpression( new CodeVariableReferenceExpression(Resources.StrParameterContractKey), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(Resources.ExcCantFindMethodEnd)))})));

			// Der GetOldValues Methode eine Exception werfen lassen wenn keine Kontraktmethode
			// gefunden wurde.
			mGetOldValuesMethodModel.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(new CodeTypeReference(typeof(ContractException)), new CodeExpression[] { new CodeBinaryOperatorExpression( new CodePrimitiveExpression(Resources.ExcCantFindMethodBegin), CodeBinaryOperatorType.Add, new CodeBinaryOperatorExpression( new CodeVariableReferenceExpression(Resources.StrParameterContractKey), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(Resources.ExcCantFindMethodEnd)))})));

			// Den CodeDomProvider instanzieren.
			CodeDomProvider provider = CodeDomProvider.CreateProvider(Microsoft.CSharp.CSharpCodeProvider.GetLanguageFromExtension(".cs"));

			CodeGeneratorOptions options = new CodeGeneratorOptions();
			// Keine Leerzeilen zwischen den Member
			options.BlankLinesBetweenMembers = false;
			// geschweifte Klammer in eigener Zeile und NICHT am Ende der zugehörigen Anweisung
			options.BracingStyle = "C";
			// Tabulator als Einrückungszeichen
			options.IndentString = "\t";
			// Alles in der Reihenfolge erzeugen, wie es im Graph steht
			options.VerbatimOrder = true;
			// nix unnützes
			options.ElseOnClosing = false;

			// Den Compiler Parameter instanziieren.
			CompilerParameters parameters = new CompilerParameters();

			// Referenzen einbinden
			// Die Contracted Assembly hinzufügen
			parameters.ReferencedAssemblies.Add(elementModel.Member.DeclaringType.Assembly.Location);
			// Alle Referenzen hinzufügen
			foreach (Assembly referenceAssembly in AssemblyController.Instance.GetReferencedAssemblies(elementModel.Member.DeclaringType.Assembly))
				parameters.ReferencedAssemblies.Add(referenceAssembly.Location);

			// Angeben, dass in jedem Fall eine Datei erzeugt werden soll.
			parameters.GenerateInMemory = false;

			// Name der Assembly angeben
			parameters.OutputAssembly = contractAssemblyFile;

#if(DEBUG)
			// Angeben dass eine Debugdatei erstellt werden soll (*.pdb)
			parameters.IncludeDebugInformation = true;
#endif

			TextWriter stringWriter = null;
			try
			{
				stringWriter = File.CreateText(sourceCodeFile);
				provider.GenerateCodeFromCompileUnit(mAssemblyCompileUnit, stringWriter, options);
			}
			finally
			{
				if (stringWriter != null)
					stringWriter.Close();
			}

			// Assembly aus Sourcecode Datei erzeugen.
			CompilerResults result = provider.CompileAssemblyFromFile(parameters, sourceCodeFile);
			if (result.Errors.Count != 0)
			{
				// Fehlermeldung ausgeben
				StringBuilder exceptionText = new StringBuilder();
				foreach (CompilerError aError in result.Errors)
				{
					exceptionText.AppendLine(aError.ErrorText);
				}
				throw new Exception(exceptionText.ToString());
			}

			// Dateien als ByteArray speichern
#if(DEBUG)
			elementModel.PdbFile = ReadFile(debugInformationFile);
			elementModel.SourceCodeFile = ReadFile(sourceCodeFile);
#endif
			elementModel.ContractAssembly = ReadFile(contractAssemblyFile);

			mSourceCodeGenerated = true;
		}

		/// <summary>
		/// Liest fullName ein und gibt ein byte Array zurück.
		/// </summary>
		/// <param name="fullName">Name der Datei die eingelesen werden soll</param>
		/// <returns>Byte Array</returns>
		private byte[] ReadFile(string fullName)
		{
			FileInfo fileInfo = new FileInfo(fullName);
			byte[] byteArray = new byte[((int)fileInfo.Length)];
			using (FileStream reader = fileInfo.OpenRead())
			{
				reader.Read(byteArray, 0, byteArray.Length);
			}

			//TODO Bitte einkommentieren.
			//fileInfo.Delete();
			return byteArray;
		}

		/// <summary>
		/// Initialisiert den ContractGenerator damit Kontrakte hinzugefügt werden können.
		/// </summary>
		/// <param name="className">Name der Klasse die erzeugt werden soll.</param>
		/// <param name="contractedAssembly">Die Assembly in der der Kontrakt definiert ist.</param>
		internal void StartContractGenerator(string className, Assembly contractedAssembly)
		{
			// Anzahl zurücksetzen
			DbcAspectCurrentCount = 0;
			// Anzahl der Kontrakte in der Assembly ermitteln
			DbcAspectCount = CountDbcAspectsInAssemblies(contractedAssembly);

			// Assembly Model erzeugen.
			mAssemblyCompileUnit = new CodeCompileUnit();

			// Namespace setzen
			CodeNamespace nameSpace = new CodeNamespace(Resources.StrNameSpace);
			//: usings;
			nameSpace.Imports.Add(new CodeNamespaceImport(Resources.StrSystemNameSpace));

			// Namespace der Assembly zuweisen
			mAssemblyCompileUnit.Namespaces.Add(nameSpace);

			//Klasse erzeugen
			mClassModel = new CodeTypeDeclaration(className);
			mClassModel.Attributes = MemberAttributes.Public;

			//Serializeable
			mClassModel.CustomAttributes.Add(new CodeAttributeDeclaration(
				new CodeTypeReference(typeof(SerializableAttribute))));

			//IContract Interface ableiten
			mClassModel.BaseTypes.Add(new CodeTypeReference(typeof(IContract)));

			//Konstruktor erzeugen
			CodeConstructor ctor = new CodeConstructor();
			ctor.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			// zum Typ hinzufügen
			mClassModel.Members.Add(ctor);

			// Die Klasse dem Namespace und dardurch auch der Assembly zuweisen.
			nameSpace.Types.Add(mClassModel);

			// Methode die die IContract Schnittstelle benötigt erzeugen.
			AddIContractInterface(mClassModel);

			// Klassengültiger Paramter setzen.
			mStarted = true;
		}

		#endregion Methoden 

	}
}
