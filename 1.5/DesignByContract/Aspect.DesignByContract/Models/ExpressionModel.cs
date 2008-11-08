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

namespace Aspect.DesignByContract.Models
{
	/// <summary>
	/// Modell das zur Konvertierung von Ausdrücken dient.
	/// </summary>
	internal class ExpressionModel
	{

		#region Interne Variablen (9) 

		/// <summary>
		/// Membervariable der Eigenschaft Element.
		/// </summary>
		private MemberInfo mElement = null;
		/// <summary>
		/// Membervariable der Eigenschaft ConvertedString.
		/// </summary>
		private string mConvertedExpression = string.Empty;
		/// <summary>
		/// Membervariable der Eigenschaft DontConvert.
		/// </summary>
		private bool mDontConvert = false;
		/// <summary>
		/// Membervariable der Eigenschaft ExpresionType.
		/// </summary>
		private Type mExpressionType = null;
		/// <summary>
		/// Membervariable der Eigenschaft GetOldValueExpressions.
		/// </summary>
		private string[] mGetOldValueExpressions = null;
		/// <summary>
		/// Membervariable der Eigenschaft IsOldValueAccess.
		/// </summary>
		private bool mIsOldValueAccess = false;
		/// <summary>
		/// Membervariable der Eigenschaft IsPrivateMethod.
		/// </summary>
		private bool mIsPrivateMethod = false;
		/// <summary>
		/// Membervariable der Eigenschaft OldValueAccessString.
		/// </summary>
		private string mOldValueAccessExpression = string.Empty;
		/// <summary>
		/// Membervariable der Eigenschaft OriginalString.
		/// </summary>
		private string mExpression = string.Empty;

		#endregion Interne Variablen 

		#region Eigenschaften (11) 

		/// <summary>
		/// Gibt das Element zurück, für das der Kontrakt definiert wurde.
		/// </summary>
		public MemberInfo Element
		{
			get { return mElement; }
			private set { mElement = value; }
		}

		/// <summary>
		/// Gibt das typisierte Element zurück, oder setzt es
		/// </summary>
		public string ConvertedExpression
		{
			get { return mConvertedExpression; }
			set { mConvertedExpression = value; }
		}

		/// <summary>
		/// Gibt den Typ zurück, welcher den ContractedMember definiert
		/// </summary>
		public Type DeclaringType
		{
			get {
				if (Element != null)
					return Element.DeclaringType;
				return null;
			}
		}

		/// <summary>
		/// Gibt an ob der Ausdruck überhaupt konvertiert werden muss.
		/// </summary>
		internal bool DontConvert
		{
			get { return mDontConvert; }
			private set { mDontConvert = value; }
		}

		/// <summary>
		/// Gibt den Typ zurück den der Ausdruck hat oder setzt ihn.
		/// </summary>
		public Type ExpressionType
		{
			get { return mExpressionType; }
			set { mExpressionType = value; }
		}

		/// <summary>
		/// Gibt eine Liste mit Ausdrücken zurück um die Alten werte zu laden.
		/// </summary>
		public string[] GetOldValueExpressions
		{
			get { return mGetOldValueExpressions; }
			private set { mGetOldValueExpressions = value; }
		}

		/// <summary>
		/// Gibt zurück ob dieser OriginalString konvertiert wird.
		/// </summary>
		public bool IsConverted
		{
			get 
			{
				if ((ConvertedExpression == null)
					|| (ConvertedExpression == string.Empty))
					return false;
				return true; 
			}
		}

		/// <summary>
		/// Gibt zurück ob es sich bei diesem Zugriff um ein zwischengespeicherten Wert handelt.
		/// </summary>
		public bool IsOldValueAccess
		{
			get { return mIsOldValueAccess; }
			private set { mIsOldValueAccess = value; }
		}

		/// <summary>
		/// Gibt an, ob das Element ein Aufruf einer Privaten Methode beinhaltet, oder setzt diese Eigenschaft.
		/// </summary>
		public bool IsPrivateMethod
		{
			get { return mIsPrivateMethod; }
			set { mIsPrivateMethod = value; }
		}

		/// <summary>
		/// Gibt den String zurück, der mit dem auf eine Recource zugegriffen werden soll um sie zwischenzuspeichern, oder setzt den String.
		/// </summary>
		public string OldValueAccessExpression
		{
			get { return mOldValueAccessExpression; }
			set { mOldValueAccessExpression = value; }
		}

		/// <summary>
		/// Gibt die originale Bezeichnung aus dem Kontrakt zurück.
		/// </summary>
		public string Expression
		{
			get { return mExpression; }
			private set { mExpression = value; }
		}

		#endregion Eigenschaften 

		#region Kon/Destructoren (Dispose) (1) 

		/// <summary>
		/// Konstruktor
		/// </summary>
		/// <param name="expression">Ausdruck der definiert wurde.</param>
		/// <param name="element">Element an dem der Kontrakt definiert wurde.</param>
		/// <param name="genericParameters">Liste mit den generischen Parametern.</param>
		internal ExpressionModel(string expression, MemberInfo element, List<string> genericParameters)
		{
			if (element == null)
				throw new NullReferenceException(Resources.ExcElementIsNull);
			if (expression.StartsWith(Resources.StrAspectParameterOldAccess))
			{
				expression = expression.Substring(5);
				IsOldValueAccess = true;
			}

			// Prüfen, ob auf den [value] Wert zugegriffen wird.
			if (expression.ToUpper() == Resources.StrAspectParameterValueAccess.ToUpper())
			{
				string valueType = string.Empty;
				if (element is FieldInfo)
					valueType = ((FieldInfo)element).FieldType.FullName;
				else if (element is PropertyInfo)
					valueType = ((PropertyInfo)element).PropertyType.FullName;
				else if ((element is MethodInfo) && (((MethodInfo)element).IsSpecialName))
					valueType = ((MethodInfo)element).ReturnType.FullName;
				DontConvert = true;
				if (string.IsNullOrEmpty(valueType))
					valueType = "object";
				ConvertedExpression = "((" + valueType + ")" + Resources.StrParameterContractArguments + "[0])";
			}

			// wenn der Ausdruck der Rückgabewert ist übersetzen.
			if (expression.ToUpper() == Resources.StrAspectParameterResultAccess.ToUpper())
			{
				string valueType = string.Empty;
				if (element is MethodInfo)
					valueType = ((MethodInfo)element).ReturnType.FullName;
				else if (element is PropertyInfo)
					valueType = ((PropertyInfo)element).PropertyType.FullName;
				DontConvert = true;
				if (string.IsNullOrEmpty(valueType))
					valueType = "object";
				ConvertedExpression = "((" + valueType + ")" + Resources.StrParameterMethodResult + ")";
			}

			string genericParameter = GetGenericType(expression, genericParameters);
			if (!string.IsNullOrEmpty(genericParameter))
			{
				DontConvert = true;
				ConvertedExpression = genericParameter;
			}

			Expression = expression;
			Element = element;
		}

		#endregion Kon/Destructoren (Dispose) 

		#region Methoden (2) 

		/// <summary>
		/// Durchsucht die Generischen Parameter ob expression einer ist. 
		/// </summary>
		/// <param name="expression">Ausdruck der unter generischem Verdacht steht.</param>
		/// <param name="genericParameters">Liste mit allen Generischen Typen.</param>
		/// <returns>String.Empty = kein generischer Typ. Sonst der Name des generischen Typs.</returns>
		private string GetGenericType(string expression, List<string> genericParameters)
		{
			if ((genericParameters != null) && (genericParameters.Count > 0))
			{
				int i = 0;
				foreach(string genericParameter in genericParameters)
				{
					if (expression == genericParameter)
					{
						return Resources.StrParameterGenericTypes + "[" + i.ToString() + "]";
					}
					i++;
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// Setzt die Liste mit Ausdrücken um ein Array zu bekommen das die alten Werte wiedergibt.
		/// </summary>
		/// <param name="getOldValueExpressions">Liste mit Ausdrücken</param>
		internal void SetGetOldValueExpression(List<string> getOldValueExpressions)
		{
			GetOldValueExpressions = new string[getOldValueExpressions.Count];
			int i = 0;
			foreach (string expr in getOldValueExpressions)
			{
				GetOldValueExpressions[i] = expr;
				i++;
			}
		}

		#endregion Methoden 

	}
}
