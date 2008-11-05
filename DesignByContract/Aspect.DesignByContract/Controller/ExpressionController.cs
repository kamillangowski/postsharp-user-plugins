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
using System.Text.RegularExpressions;
using System.Globalization;
using Aspect.DesignByContract.Models;
using Aspect.DesignByContract.Properties;

namespace Aspect.DesignByContract.Controller
{
	/// <summary>
	/// Controller zum Konvertieren von Ausdrücken.
	/// </summary>
	internal class ExpressionController
	{

		#region Interne Variablen (2) 

		/// <summary>
		/// Liste mit den Namen der generischen Typen für die Klasse in der der Kontrakt definiert wurde.
		/// </summary>
		private List<string> mGenericParameter = null;
		/// <summary>
		/// Liste mit Ausdrücken, die in die GetOldValue Methode eingebunden werden sollen.
		/// </summary>
		private List<string> mOldValueExpressions = null;

		#endregion Interne Variablen 

		#region Kon/Destructoren (Dispose) (1) 

		/// <summary>
		/// Konstruktor
		/// </summary>
		internal ExpressionController()
		{
		}

		#endregion Kon/Destructoren (Dispose) 

		#region Methoden (17) 

		/// <summary>
		/// Typisiert den übergebenen Ausdruck.
		/// </summary>
		/// <param name="expression">Der zu Typisierende Ausdruck.</param>
		/// <param name="element">Element an dem der Kontrakt definiert wurde.</param>
		/// <returns>
		/// Ein ExpressionModel mit dem übersetzten Ausdruck. Wenn der Ausdruck nicht Typisiert 
		/// werden konnte wird einfach der originale Wert zum konventierten.</returns>
		private ExpressionModel ConvertElementExpression(string expression, MemberInfo element)
		{
			ExpressionModel elementExpressionModel = new ExpressionModel(expression, element, mGenericParameter);

			// Wenn das Model aus welchem Grund auch immer nicht Konvertiert werden soll, einfach 
			// zurückgeben
			if (elementExpressionModel.DontConvert)
				return elementExpressionModel;

			// wenn expression ein Reservierter ausdruck ist einfach zurückgeben.
			if (IsDefinedExpression(elementExpressionModel.Expression))
			{
				elementExpressionModel.ConvertedExpression = elementExpressionModel.Expression;
				return elementExpressionModel;
			}

			// Prüfen ob der Ausdruck ein String ist.
			ExpressionModel checkExprnMdl = ConvertToStringExpression(elementExpressionModel);
			if (checkExprnMdl != null)
				return checkExprnMdl;

			// Prüfen ob der Ausdruck ein Übergabeparameter ist.
			checkExprnMdl = ConvertToParameterExpression(elementExpressionModel);
			if (checkExprnMdl != null)
			{
				if (checkExprnMdl.IsOldValueAccess)
					checkExprnMdl = GetOldValueExpression(checkExprnMdl, true);
				return checkExprnMdl;
			}

			// Prüfen ob der Ausdruck ein zugriff auf einen public Member der Klasse ist, in der der
			// Kontrakt definiert ist.
			checkExprnMdl = ConvertToPublicMemberExpression(elementExpressionModel);
			if (checkExprnMdl != null)
			{
				if (checkExprnMdl.IsOldValueAccess)
					checkExprnMdl = GetOldValueExpression(checkExprnMdl, true);
				return checkExprnMdl;
			}

			// Prüfen ob der Ausdruck ein zugriff auf einen private Member der Klasse ist, in der der
			// Kontrakt definiert ist.
			checkExprnMdl = ConvertToPrivateMemberExpression(elementExpressionModel, BindingFlags.NonPublic | BindingFlags.Instance );
			if (checkExprnMdl != null)
			{
				if (checkExprnMdl.IsOldValueAccess)
					checkExprnMdl = GetOldValueExpression(checkExprnMdl, true);
				return checkExprnMdl;
			}

			checkExprnMdl = ConvertToTypedExpression(elementExpressionModel);
			if (checkExprnMdl != null)
				return checkExprnMdl;

			// Da der Ausdruck nicht übersetzt werden konnte wird der Original Ausdruck gesetzt.
			elementExpressionModel.ConvertedExpression = elementExpressionModel.Expression;
			return elementExpressionModel;
		}

		/// <summary>
		/// Konvertiert einen Ausdruck und liefert einen Expression Model zurück.
		/// </summary>
		/// <param name="expression">Der zu konvertierende Ausdruck.</param>
		/// <param name="element">Das element an dem der Kontrakt definiert wurde.</param>
		/// <returns>Ein Expression Model.</returns>
		internal ExpressionModel ConvertExpression(string expression, MemberInfo element)
		{
			if (String.IsNullOrEmpty(expression))
				throw new NullReferenceException(Resources.ExcContractIsNull);
			if (element == null)
				throw new NullReferenceException(Resources.ExcContractedMemberIsNull);

			// this. ist ein zugriff auf ein Klassenelement this. kann jedoch nicht aufgelöst werden
			// daher wird this. entfernt.
			expression.Replace("this.", "");

			// Wenn es sich um einen Generics Typ handelt müssen alle Generics Parameter ersetzt werden.
			if (element.DeclaringType.IsGenericType)
			{
				mGenericParameter = new List<string>();
				Type[] genericTypes = element.DeclaringType.GetGenericArguments();
				foreach (Type genericParameterType in genericTypes)
                {
					if (genericParameterType.IsGenericParameter)
						mGenericParameter.Add(genericParameterType.Name);
					else
						throw new ArgumentException (string.Format(CultureInfo.CurrentCulture, Resources.ExcNoGenericParameter, new object[]{genericParameterType}));
                }
			}

			// die Liste für den Zugriff auf gespeicherte (alte) Werte zurücksetzen.
			mOldValueExpressions = new List<string>();

			ExpressionModel exprModel = new ExpressionModel(expression, element, mGenericParameter);
			exprModel.ConvertedExpression = GetConvertedExpression(expression, element);

			if (mOldValueExpressions.Count > 0)
			{
				mOldValueExpressions.Insert(0, "object[] " + Resources.StrParameterOldValues + " = new object[" + mOldValueExpressions.Count + "]");
				mOldValueExpressions.Add("return " + Resources.StrParameterOldValues );
				foreach (string oldValue in mOldValueExpressions)
					exprModel.OldValueAccessExpression += "\r\n" + oldValue;
				exprModel.SetGetOldValueExpression(mOldValueExpressions);
			}

			return exprModel;

		}

		/// <summary>
		/// Prüft ob der übergebene Ausdruck ein Übergabeparameter ist und setzt alle Typbasierten Paramter. 
		/// Wenn der übergebene Ausdruck kein Übergabeparameter ist wird null zurückgegeben.
		/// </summary>
		/// <param name="expression">Der zu prüfende Ausdruck</param>
		/// <returns>Ein ExpressionModel wenn der übergebene Ausdruck ein Übergabeparameter ist, andernfalls null </returns>
		private ExpressionModel ConvertToParameterExpression(ExpressionModel expression)
		{
			ParameterInfo[] parameterInfos = null;

			if (expression.Element is MethodInfo)
				parameterInfos = ((MethodInfo)expression.Element).GetParameters();
			else if (expression.Element is PropertyInfo)
				parameterInfos = ((PropertyInfo)expression.Element).GetSetMethod(true).GetParameters();
			if (parameterInfos == null)
				return null;
			foreach (ParameterInfo paraInfo in parameterInfos)
			{
				if (paraInfo.Name == expression.Expression)
				{
					expression.ExpressionType = paraInfo.ParameterType;
					expression.ConvertedExpression = GetArrayExpression(paraInfo.ParameterType.FullName, Resources.StrParameterContractArguments, paraInfo.Position);
					return expression;
				}
			}
			return null;
		}

		/// <summary>
		/// Prüft ob der übergebene Ausdruck ein Private Member der Klasse, in der der Kontrakt 
		/// angegenben ist und setzt alle Typbasierten Paramter. Wenn der übergebene Ausdruck 
		/// kein Private Member der Klasse, in der der Kontrakt angegenben ist wird null 
		/// zurückgegeben.
		/// </summary>
		/// <param name="expression">Der zu prüfende Ausdruck</param>
		/// <returns>Ein ExpressionModel wenn der übergebene Ausdruck ein Private Member der Klasse, 
		/// in der der Kontrakt angegenben ist, andernfalls null </returns>
		private ExpressionModel ConvertToPrivateMemberExpression(ExpressionModel expression, BindingFlags bindingFlags)
		{
			// Sucht ob expression der Name einer privaten Methode ist.
			MethodInfo method = null;
			// BUG PostSharp Version 1.5 liefert nicht die Methoden zurück.
			// method = expression.DeclaringType.GetMethod(expression.Expression, bindingFlags);
			// woraround
			method = GetMethodInfo(expression.Expression, expression.DeclaringType, bindingFlags);
			string memberType = string.Empty;
			if (method != null)
			{
				if (method.ReturnType != null)
					expression.ExpressionType = method.ReturnType;
				else
					throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.ExcMethodIsVoid, new object[] { method.Name }));

				memberType = expression.ExpressionType.FullName;

				if (string.IsNullOrEmpty(memberType))
					memberType = "object";
				expression.ConvertedExpression = "((" + memberType + ")(" + Resources.StrParameterInstance + ".GetType().GetMethod(\"" + expression.Expression + "\", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(" + Resources.StrParameterInstance + ", ";

				expression.IsPrivateMethod = true;
				return expression;
			}

			// Sucht ob expression der Name einer privaten Eigenschaft ist.
			PropertyInfo property = null;
			// BUG PostSharp Version 1.5 liefert nicht die Eigenschaften zurück.
			// property = expression.DeclaringType.GetProperty(expression.Expression, bindingFlags);
			// Woraround
			property = GetPropertyInfo(expression.Expression, expression.DeclaringType, bindingFlags);

			if (property != null)
			{
				Type propertyType = null;
				if (property.GetGetMethod(true) != null)
					propertyType = property.GetGetMethod(true).ReturnType;
				else
					throw new Exception(string.Format ( CultureInfo.CurrentCulture, Resources.ExcNoGetMethodAvailable, new object[]{property.Name}));
				expression.ExpressionType = propertyType;
				memberType = expression.ExpressionType.FullName;
				if (string.IsNullOrEmpty(memberType))
					memberType = "object";
				expression.ConvertedExpression = "((" + memberType + ")(" + Resources.StrParameterInstance + ".GetType().GetProperty(\"" + expression.Expression + "\", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue( " + Resources.StrParameterInstance + ", null )))";

				return expression;
			}

			// Sucht ob expression der Name eines privaten Feldes ist.
			FieldInfo field = null;
			// BUG PostSharp Version 1.5 liefert nicht die Felder zurück.
			// field = expression.DeclaringType.GetField(expression.Expression, bindingFlags);
			// Workaround
			field = GetFieldInfo(expression.Expression, expression.DeclaringType, bindingFlags);
			if (field != null)
			{
				expression.ExpressionType = field.FieldType;
				memberType = expression.ExpressionType.FullName;

				if (string.IsNullOrEmpty(memberType))
					memberType = "object";
				expression.ConvertedExpression = "((" + memberType + ")(" + Resources.StrParameterInstance + ".GetType().GetField(\"" + expression.Expression + "\", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue( " + Resources.StrParameterInstance + " )))";
				return expression;
			}

			return null;
		}

		/// <summary>
		/// Prüft ob der übergebene Ausdruck ein Public Member der Klasse, in der der Kontrakt 
		/// angegenben ist und setzt alle Typbasierten Paramter. Wenn der übergebene Ausdruck 
		/// kein Public Member der Klasse, in der der Kontrakt angegenben ist wird null 
		/// zurückgegeben.
		/// </summary>
		/// <param name="expression">Der zu prüfende Ausdruck</param>
		/// <returns>Ein ExpressionModel wenn der übergebene Ausdruck ein Public Member der Klasse, 
		/// in der der Kontrakt angegenben ist, andernfalls null </returns>
		private ExpressionModel ConvertToPublicMemberExpression(ExpressionModel expression)
		{
			// Wenn der Typ in dem der Kontrakt definiert ist schon nicht Public ist, muss via Reflection
			// Auf das Element zugegriffen werden.
			if (!expression.DeclaringType.IsPublic)
				return ConvertToPrivateMemberExpression(expression, BindingFlags.Public | BindingFlags.Instance);

			MemberInfo[] memberInfos = null;
			// BUG PostSharp Version 1.5 liefert nicht die Mitglieder zurück.
			// MemberInfo[] memberInfos = null;
			// memberInfos = expression.DeclaringType.GetMember(expression.Expression, BindingFlags.Public | BindingFlags.Instance);
			// Workaround
			MemberInfo tempMemberInfo = GetMemberInfo(expression.Expression, expression.DeclaringType, BindingFlags.Public | BindingFlags.Instance);
			if (tempMemberInfo != null)
				memberInfos = new MemberInfo[] { tempMemberInfo };

			if ((memberInfos != null)
				&& (memberInfos.Length > 0))
			{
				if (memberInfos.Length != 1)
					throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.ExcMorePublicMemberFound, new object[] { expression.Expression }));
				MemberInfo memberInfo = memberInfos[0];
				if (memberInfo is PropertyInfo)
				{
					if (((PropertyInfo)memberInfo).GetGetMethod(true) != null)
						expression.ExpressionType = ((PropertyInfo)memberInfo).GetGetMethod(true).ReturnType;
					else
						throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.ExcNoGetMethodAvailable, new object[] { ((PropertyInfo)memberInfo).Name }));
				}
				else if (memberInfo is MethodInfo)
				{
					if (((MethodInfo)memberInfo).ReturnType != null)
						expression.ExpressionType = ((MethodInfo)memberInfo).ReturnType;
					else
						throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.ExcMethodIsVoid, new object[] { ((MethodInfo)memberInfo).Name }));
				}
				else if (memberInfo is FieldInfo)
					expression.ExpressionType = ((FieldInfo)memberInfo).FieldType;
				string expressionType = expression.ExpressionType.FullName;
				if (string.IsNullOrEmpty(expressionType))
					expressionType = "object";
				expression.ConvertedExpression = "(("+expression.DeclaringType+")" + Resources.StrParameterInstance + ")." + expression.Expression;
				return expression;
			}
			return null;
		}

		/// <summary>
		/// Prüft ob der übergebene Ausdruck ein String ist und setzt alle Typbasierten Paramter. 
		/// Wenn der übergebene Ausdruck kein String ist wird null zurückgegeben.
		/// </summary>
		/// <param name="expression">Der zu prüfende Ausdruck</param>
		/// <returns>Ein ExpressionModel wenn der übergebene Ausdruck ein String ist, andernfalls null </returns>
		private ExpressionModel ConvertToStringExpression(ExpressionModel expression)
		{
			if ((expression.Expression.StartsWith("\""))
				&& (expression.Expression.EndsWith("\"")))
			{
				expression.ExpressionType = typeof(string);
				expression.ConvertedExpression = expression.Expression;
				return expression;
			}
			return null;
		}

		/// <summary>
		/// Prüft ob der übergebene Ausdruck ein Type ist und setzt alle Typbasierten Paramter. 
		/// Wenn der übergebene Ausdruck kein Type ist wird null zurückgegeben.
		/// </summary>
		/// <param name="expression">Der zu prüfende Ausdruck</param>
		/// <returns>Ein ExpressionModel wenn der übergebene Ausdruck ein Type ist, andernfalls null </returns>
		private ExpressionModel ConvertToTypedExpression(ExpressionModel expression)
		{
			Type convertedType = AssemblyController.Instance.FindTypeInAssemblyAndAllReferences(expression.Expression, expression.DeclaringType.Assembly);

			if (convertedType != null)
			{
				expression.ExpressionType = convertedType;
				expression.ConvertedExpression = expression.ExpressionType.FullName;
				return expression;
			}
			return null;
		}

		/// <summary>
		/// Baut einen Ausdruck für den zugriff auf ein Array zusammen.
		/// </summary>
		/// <param name="typeName">Der konkrete Typ des Array Elements</param>
		/// <param name="arrayName">Name des Arrays</param>
		/// <param name="position">Die Arrayelement Postition die Aufgerufen werden soll.</param>
		/// <returns>Der zusammengebaute ausdrucks String für ein Zugriff auf ein Element.</returns>
		private string GetArrayExpression(string typeName, string arrayName, int position)
		{
			StringBuilder aStringBuilder = new StringBuilder();

			aStringBuilder.Append("(");
			if ((typeName != null)
				&& (typeName != string.Empty))
			{
				aStringBuilder.Append("(");
				aStringBuilder.Append(typeName);
				aStringBuilder.Append(")");
			}
			aStringBuilder.Append(arrayName);
			aStringBuilder.Append("[");
			aStringBuilder.Append(position);
			aStringBuilder.Append("])");
			return aStringBuilder.ToString();
		}

		/// <summary>
		/// Aufsplitung des Ausdrucks findet hier statt. Rekursier Aufruf ist möglich.
		/// </summary>
		/// <param name="expression">Der zu konvertierende Ausdruck.</param>
		/// <param name="element">Element an dem der Kontrakt definiert wurde.</param>
		/// <returns>Den konvertierenten Ausdruck.</returns>
		private string GetConvertedExpression(string expression, MemberInfo element)
		{
			if ((expression.Length == 0)
				|| (expression.Trim().Length == 0))
				return expression;

			//Die Position des Nächsten Element finden
			int nextElementPosition = GetNextElementPosition(expression);

			//Postion des nächsten Member ermitteln
			int nextMemberPosition = expression.IndexOf(".");
			//prüfen ob der übergebene Ausdruck nicht übersetzt werden muss Beispiel: ").ToString()*"
			// dieser Teil müsste nicht übersetzt werden. Daher ein rekursiven Aufruf ab ")*"
			if (nextElementPosition == 0)
			{
				int cutLength = 1;
				if (nextMemberPosition == 1)
					cutLength += GetNextElementPosition(expression.Substring(cutLength));
				return expression.Substring(0, cutLength) + GetConvertedExpression(expression.Substring(cutLength), element);
			}

			// Prüfen ob expresion ein Memberaufruf ist Beispiel: "FeldKlasse.Member"
			if ((nextMemberPosition >= 0) && (nextMemberPosition < nextElementPosition))
			{
				//Den Typ in dem es noch Memberaufrufe gibt Identifizieren
				ExpressionModel memberExpressionType = ConvertElementExpression(expression.Substring(0, nextMemberPosition), element);
				//Es gibt kein nächstes Element daher kann der String bis zum Ende angefügt werden
				if (nextElementPosition == int.MaxValue)
				{
					memberExpressionType.ConvertedExpression += expression.Substring(nextMemberPosition);
					if (memberExpressionType.IsOldValueAccess)
						memberExpressionType = GetOldValueExpression(memberExpressionType, false);
				}
				else
				{
					// den String von der MemberPosition bis zur nächsten ElementPosition abschneiden
					// Beispiel: "*.Member.MemberMethod(*" abschneiden zu "*.Member.MemberMethod*"
					memberExpressionType.ConvertedExpression += expression.Substring(nextMemberPosition, nextElementPosition - nextMemberPosition);
					// Prüfen ob das nächteElement der Anfang einer Methode ist, dann müssen die
					// Übergabeparameter herausselektiert werden und übersetzt werden. Grund dafür ist
					// die Möglichkeit, dass der Eigentliche Member der Später eine Methode aufruft
					// als OldValue definiert ist. und dazu gehören dann auch die Übergabeparameter.
					if (expression.Substring(nextElementPosition, 1) == "(")
					{
						int endOfMethodPosition = 0;
						string currentNextElement = string.Empty;
						// Den übergabeparameter bereich finden Beispiel "*(para1, para2)*" = "para1, para2"
						do
						{
							endOfMethodPosition = endOfMethodPosition + GetPositionOfMethodEnd(expression.Substring(endOfMethodPosition)) + 1;
							int checkElementPos = GetNextElementPosition(expression.Substring(endOfMethodPosition));
							if (checkElementPos == int.MaxValue)
								break;
							endOfMethodPosition += checkElementPos;
							if (endOfMethodPosition == expression.Length)
								break;
							currentNextElement = expression.Substring(endOfMethodPosition, 1);
						} while ((currentNextElement == ")")
								|| (currentNextElement == "("));
						
						// Dem bis jetzt konvertiertem String die konvertierent Parameter hinzufügen
						memberExpressionType.ConvertedExpression += GetConvertedExpression(expression.Substring(nextElementPosition, (endOfMethodPosition - nextElementPosition)), element);
						// wenn der Ausdruck mit [old] gekennzeichnet wurde als ArrayAusdruck 
						// übersetzen
						if (memberExpressionType.IsOldValueAccess)
							memberExpressionType = GetOldValueExpression(memberExpressionType, false);
						// Den Rest vom Ausdruck (nach den Parametern) anfügen
						memberExpressionType.ConvertedExpression += GetConvertedExpression(expression.Substring(endOfMethodPosition), element);
					}
					else
					{
						// wenn der Ausdruck mit [old] gekennzeichnet wurde als ArrayAusdruck 
						// übersetzen
						if (memberExpressionType.IsOldValueAccess)
							memberExpressionType = GetOldValueExpression(memberExpressionType, false);
						// Den Rest vom Ausdruck (nach den Parametern) anfügen
						memberExpressionType.ConvertedExpression += GetConvertedExpression(expression.Substring(nextElementPosition), element);
					}
				}
				return memberExpressionType.ConvertedExpression;
			}
			// Gibt es noch ein weiteren Ausdruck im Beispiel "*member1 < member2*" = "member1"
			else if (nextElementPosition < int.MaxValue)
			{

				ExpressionModel convertedExpression = ConvertElementExpression(expression.Substring(0, nextElementPosition), element);

				// Wenn der gerade übersetzte Ausdruck eine private Methode ist muss er noch 
				// abgeschlossen werden, da die übergabeParamter nicht übersetz wurden da das "("
				// Zeichen den String abschneidet, was bei einer Public Methode O.K. ist jedoch bei
				// einer privaten nicht da über GetMethod(METHODE).Invoke(INSTANCE, ARGUMENTE) zur 
				// Laufzeit auf die Methode zugegriffen wird.
				if (convertedExpression.IsPrivateMethod)
				{
					int lengthTillCloseMethod = GetPositionOfMethodEnd(expression.Substring(nextElementPosition));
					string methodArgs = expression.Substring(nextElementPosition + 1, lengthTillCloseMethod - 1);
					// Die Private Method hat keine Übergabeparameter
					if (methodArgs.Trim().Length <= 0)
					{
						return convertedExpression.ConvertedExpression + "null)))" + GetConvertedExpression( expression.Substring(nextElementPosition + lengthTillCloseMethod + 1), element);
					}
					// Die Private Methode hat Übergabeparameter
					else
					{
						// Übergabeparameter übersetzen
						methodArgs = convertedExpression.ConvertedExpression + "new object[]{";
						methodArgs += GetConvertedExpression(expression.Substring(nextElementPosition + 1, lengthTillCloseMethod-1), element)+ "}))";
						// Alles was nach den Übergabeparametern in diesem Ausdruck kommt, übersetzen
						// Aber erst nach dem abhschießenden ) da dies schon oben hinzugrfügt wurde.
						int restOfExpression = (lengthTillCloseMethod + nextElementPosition);
						if (restOfExpression < expression.Length)
							methodArgs += GetConvertedExpression(expression.Substring(restOfExpression), element);
						return methodArgs;
					}
				}
				return convertedExpression.ConvertedExpression + GetConvertedExpression(expression.Substring(nextElementPosition), element);
			}
			return ConvertElementExpression(expression, element).ConvertedExpression;
		}

		/// <summary>
		/// Workaround um die FieldInfo von fieldName zu laden.
		/// </summary>
		/// <param name="fieldName">Name des Feldes das gefunden werden muss.</param>
		/// <param name="declaringType">Typ in dem gesucht werden muss.</param>
		/// <param name="bindingFlags">Ob private oder public gefunden werden soll</param>
		/// <returns>FieldInfo -> erlogreich gefunden. null -> nichts gefunden</returns>
		private FieldInfo GetFieldInfo(string fieldName, Type declaringType, BindingFlags bindingFlags)
		{
			FieldInfo[] fieldInfos = declaringType.GetFields(bindingFlags);
			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				if (fieldInfo.Name != fieldName)
					continue;
				if (((bindingFlags & BindingFlags.Public) != BindingFlags.Public)
					&& ((bindingFlags & BindingFlags.NonPublic) != BindingFlags.NonPublic))
					return fieldInfo;
				else if (((bindingFlags & BindingFlags.Public) == BindingFlags.Public) && (fieldInfo.IsPublic))
					return fieldInfo;
				else if ((bindingFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic)
					return fieldInfo;
			}
			return null;
		}

		/// <summary>
		/// Workaround um die MemberInfo von methodName zu laden.
		/// </summary>
		/// <param name="memberName">Name des Mitglieds das gefunden werden muss.</param>
		/// <param name="declaringType">Typ in dem gesucht werden muss.</param>
		/// <param name="bindingFlags">Ob private oder public gefunden werden soll</param>
		/// <returns>MemberInfo -> erlogreich gefunden. null -> nichts gefunden</returns>
		private MemberInfo GetMemberInfo(string memberName, Type declaringType, BindingFlags bindingFlags)
		{
			MemberInfo memberInfo = null;
			memberInfo = GetPropertyInfo(memberName, declaringType, bindingFlags);
			if (memberInfo != null)
				return memberInfo;
			memberInfo = GetMethodInfo(memberName, declaringType, bindingFlags);
			if (memberInfo != null)
				return memberInfo;
			memberInfo = GetFieldInfo(memberName, declaringType, bindingFlags);
			return memberInfo;
		}

		/// <summary>
		/// Workaround um die MethodInfo von methodName zu laden.
		/// </summary>
		/// <param name="methodName">Name der Methode die gefunden werden muss.</param>
		/// <param name="declaringType">Typ in dem gesucht werden muss.</param>
		/// <param name="bindingFlags">Ob private oder public gefunden werden soll</param>
		/// <returns>MethodInfo -> erlogreich gefunden. null -> nichts gefunden</returns>
		private MethodInfo GetMethodInfo(string methodName, Type declaringType, BindingFlags bindingFlags)
		{
			MemberInfo[] memberInfos = declaringType.GetMembers();
			foreach (MemberInfo memberInfo in memberInfos)
			{
				if ((memberInfo.Name != methodName) || (!(memberInfo is MethodInfo)))
					continue;
				MethodInfo methodInfo = memberInfo as MethodInfo;
				if (((bindingFlags & BindingFlags.Public) != BindingFlags.Public) 
					&& ((bindingFlags & BindingFlags.NonPublic) != BindingFlags.NonPublic))
					return methodInfo;
				else if (((bindingFlags & BindingFlags.Public) == BindingFlags.Public) && (methodInfo.IsPublic))
					return methodInfo;
				else if ((bindingFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic)
					return methodInfo;
			}
			return null;
		}

		/// <summary>
		/// Workaround um die PropertyInfo von propertyName zu laden.
		/// </summary>
		/// <param name="propertyName">Name der Eigenschft die gefunden werden muss.</param>
		/// <param name="declaringType">Typ in dem gesucht werden muss.</param>
		/// <param name="bindingFlags">Ob private oder public gefunden werden soll</param>
		/// <returns>PropertyInfo -> erlogreich gefunden. null -> nichts gefunden</returns>
		private PropertyInfo GetPropertyInfo(string propertyName, Type declaringType, BindingFlags bindingFlags)
		{
			PropertyInfo[] propertyInfos = declaringType.GetProperties(bindingFlags);
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				if (propertyInfo.Name != propertyName)
					continue;
				if (((bindingFlags & BindingFlags.Public) != BindingFlags.Public)
					&& ((bindingFlags & BindingFlags.NonPublic) != BindingFlags.NonPublic))
					return propertyInfo;
				else if (((bindingFlags & BindingFlags.Public) == BindingFlags.Public) && (propertyInfo.GetGetMethod().IsPublic))
					return propertyInfo;
				else if ((bindingFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic)
					return propertyInfo;
			}
			return null;
		}

		/// <summary>
		/// Sucht ein Zeichen welches das Ende eines Elements darstellt.
		/// </summary>
		/// <param name="expression">Der zu durchsuchende Ausdruck.</param>
		/// <returns>Position an der das Zeichen steht. Wenn es kein Zeichen gibt ist der Wert int.MaxValue</returns>
		private int GetNextElementPosition(string expression)
		{
			// Zeichen die ein neues Element signalisieren.
			string[] charsToSplit = new string[] { ",", " ", "(", ")", "=", "!", "<", ">", "&", "{", "}", "[", "]" };
			// Definierte Zeichen des DesignByContract Aspects die auch ein 
			// charToSplit beinhalten aber nicht berücksichtigt werden dürfen.
			List<string> charsToIgnore = new List<string>();
			charsToIgnore.AddRange(new string[]{ Resources.StrAspectParameterOldAccess, Resources.StrAspectParameterValueAccess, Resources.StrAspectParameterResultAccess });

			// Die charsToIgnore aus expression entfernen. Beispiel: [old]testMember -> aaatestMember
			foreach (string ignoreString in charsToIgnore)
			{
				int indexOfIgnoreString = expression.ToUpper().IndexOf(ignoreString.ToUpper());
				string replaceString = string.Empty;
				if (indexOfIgnoreString >= 0)
				{
					for (int i = 0; i < ignoreString.Length; i++)
						replaceString += "a";

					// da ein string vielleicht öfter vo kommen kann.
					while (indexOfIgnoreString >= 0)
					{
						expression = expression.Substring(0, indexOfIgnoreString) + replaceString + expression.Substring(indexOfIgnoreString + ignoreString.Length);
						indexOfIgnoreString = expression.IndexOf(ignoreString);
					}
				}
			}

			// Das erste Vorkommen eines charsToSplit finden.
			int position = int.MaxValue;
			foreach (string charToSplit in charsToSplit)
			{
				int checkposition = expression.IndexOf(charToSplit);
				if ((checkposition >= 0)
					&& (position > checkposition))
				{
					position = checkposition;
				}
			}
			return position;
		}

		/// <summary>
		/// Konvertiert einen Ausdruck zu einem Arrayzugriff. Das Array repräsentiert alle Werte die
		/// mit [old] gekennzeichnet wurden.
		/// </summary>
		/// <param name="expression">Der zu konvertierende Ausdruck</param>
		/// <param name="convertToType">Gibt an ob der Zugriff auf das OldValue Array typisiert werden soll</param>
		/// <returns>Das ExpressionModel mit zugriff auf das OldValue Array</returns>
		private ExpressionModel GetOldValueExpression(ExpressionModel expression, bool convertToType)
		{
			mOldValueExpressions.Add(Resources.StrParameterOldValues+"[" + mOldValueExpressions.Count.ToString() + "] = " + expression.ConvertedExpression);
			expression.OldValueAccessExpression = expression.ConvertedExpression;
			string typOfValue = (convertToType&& expression.ExpressionType != null) ? expression.ExpressionType.FullName : string.Empty;

			expression.ConvertedExpression = GetArrayExpression(typOfValue, Resources.StrParameterOldValues, (mOldValueExpressions.Count-1));
			return expression;
		}

		/// <summary>
		/// Sucht die Position der abschließenden Klammer einer übergebenen Methode.
		/// </summary>
		/// <param name="methodExpression">Der Methodenaufrufsausdruck</param>
		/// <returns>Position der Abschließenden Klammer</returns>
		private int GetPositionOfMethodEnd(string methodExpression)
		{
			int numberOfOpenMethods = 0;
			int indexOfOpenMethod = int.MinValue;
			int indexOfCloseMethod = int.MinValue;
			int startIndex = 0;
			do
			{
				indexOfOpenMethod = methodExpression.IndexOf("(", startIndex);
				indexOfCloseMethod = methodExpression.IndexOf(")", startIndex);
				if ((indexOfOpenMethod >= 0)
					&& (indexOfOpenMethod < indexOfCloseMethod))
				{
					startIndex = indexOfOpenMethod + 1;
					numberOfOpenMethods++;
				}
				else
				{
					startIndex = indexOfCloseMethod + 1;
					numberOfOpenMethods--;
				}
			} while (numberOfOpenMethods > 0);
			return indexOfCloseMethod;
		}

		/// <summary>
		/// Gibt an, ob der Wert (value) ein definierter Ausdruck ist.
		/// </summary>
		/// <param name="value">Wert der geprüft werden soll.</param>
		/// <returns>true=ein definierter Wert, false=kein definierter Wert</returns>
		private bool IsDefinedExpression(string value)
		{
			string[] defindedCSharpExpressions = new string[] { "is", "true", "false", "null", "new" };
			foreach (string definedExpression in defindedCSharpExpressions)
			{
				if (value == definedExpression)
					return true;
			}
			return false;
		}

		#endregion Methoden 

	}
}
