#region Using Statements

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
    /// <summary>
    /// Custom attribute that, when applied (or multicasted) to a field, 
    /// writes an informative record
    /// to the <see cref="Trace"/> class every time a this field is read or written.
    /// </summary>
    [Serializable]
    public sealed class GTraceField : OnFieldAccessAspect, IMessageSourceProvider
    {
        // Tracing enabled by default, but can be enabled/disabled at run-time
        private static bool _enabled = true;

        /// <summary>
        /// Enables or disables tracing.  Note that tracing is enabled by default.
        /// </summary>
        public static bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        private string _classFullName;
        private string _className;
        private string _methodName;
        private string _qualifiedMethodName;
        private string _setFormatString;

        /// <summary>
        /// Initializes the current object. Called at compile time by PostSharp.
        /// </summary>
        /// <param name="field">Field to which the current instance is
        /// associated.</param>
        public override void CompileTimeInitialize(FieldInfo field)
        {
            _className = field.DeclaringType.Name;
            _classFullName = field.DeclaringType.Namespace + "." + _className;
            _methodName = field.Name;
            _qualifiedMethodName = _className + "." + _methodName;

            if (field.IsStatic)
                _setFormatString = _qualifiedMethodName + " = {0}";
            else
                _setFormatString = "{1}::" + _qualifiedMethodName + " = {0}";
        }

        /// <summary>
        /// Method called instead of the <i>set</i> operation on the modified field. 
        /// We just write a record to the trace subsystem and set the field value.
        /// </summary>
        /// <param name="context">Event arguments specifying which field is being
        /// accessed and which is its current value, and allowing to change its value.
        /// </param>
        public override void OnSetValue(FieldAccessEventArgs context)
        {
            // Get out fast if tracing is disabled
            if (!Enabled)
                return;

            if ((context.StoredFieldValue == null && context.ExposedFieldValue == null)
                || (context.StoredFieldValue == null || !context.StoredFieldValue.Equals(context.ExposedFieldValue)))
            {
                string caption = FormatString(_setFormatString, context.ExposedFieldValue ?? "<null>", context.Instance);
                string description = "was: " + context.StoredFieldValue ?? "<null>";
                Log.Write(LogMessageSeverity.Verbose, "PostSharp", this, null, null, LogWriteMode.Queued, null,
                          "PostSharp.Set", GAspectBase.Indent(caption), description);
            }
            base.OnSetValue(context);
        }

        /// <summary>
        /// Gets a formatting string representing a <see cref="Type"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>A formatting string representing the type
        /// where each generic type argument is represented as a
        /// formatting argument (e.g. <c>Dictionary&lt;{0},P1}&gt;</c>.
        /// </returns>
        public static string GetTypeFormatString(Type type)
        {
            StringBuilder stringBuilder = new StringBuilder();

            // Build the format string for the declaring type.

            stringBuilder.Append(type.FullName);

            if (type.IsGenericTypeDefinition)
            {
                stringBuilder.Append("<");
                for (int i = 0; i < type.GetGenericArguments().Length; i++)
                {
                    if (i > 0)
                        stringBuilder.Append(", ");
                    stringBuilder.AppendFormat("{{{0}}}", i);
                }
                stringBuilder.Append(">");
            }
            return stringBuilder.ToString();
        }

        public static string FormatString(string format, params object[] args)
        {
            return args == null ? format : string.Format(format, args);
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
    }
}