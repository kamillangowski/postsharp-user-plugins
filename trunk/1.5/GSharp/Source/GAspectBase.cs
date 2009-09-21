#region Released to Public Domain by eSymmetrix, Inc.

/********************************************************************************
 *   This file is sample code demonstrating Gibraltar integration with PostSharp
 *   
 *   This sample is free software: you have an unlimited rights to
 *   redistribute it and/or modify it.
 *   
 *   This sample is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 *   
 *******************************************************************************/

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Gibraltar.Agent;
using PostSharp.Laos;

#endregion

namespace Gibraltar.GSharp
{
    [Serializable]
    public abstract class GAspectBase : OnMethodBoundaryAspect, IMessageSourceProvider
    {
        private const int IndentSize = 2;
        private string _classFullName;
        private string _className;
        private string _methodName;
        private string _qualifiedMethodName;

        public string QualifiedMethodName
        {
            get { return _qualifiedMethodName; }
        }

        #region IMessageSourceProvider

        public string MethodName
        {
            get { return _methodName; }
        }

        public string ClassName
        {
            get { return _classFullName; }
        }

        public string FileName
        {
            get { return null; }
        }

        public int LineNumber
        {
            get { return 0; }
        }

        #endregion

        public override void CompileTimeInitialize(MethodBase method)
        {
            _className = method.DeclaringType.Name;
            _classFullName = method.DeclaringType.Namespace + "." + _className;
            _methodName = method.Name;
            _qualifiedMethodName = _className + "." + _methodName;
        }

        public static string Indent(string text, int indentLevel)
        {
            // Use indentation to show nesting level
            StringBuilder builder = new StringBuilder();
            builder.Append(' ', indentLevel*IndentSize);
            builder.Append(text);
            return builder.ToString();
        }

        public static string Indent(string text)
        {
            return Indent(text, Trace.IndentLevel);
        }
    }
}