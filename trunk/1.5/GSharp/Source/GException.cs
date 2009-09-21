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
using System.Reflection;
using Gibraltar.Agent;
using PostSharp.Laos;

#endregion File Header

namespace Gibraltar.GSharp
{
    /// <summary>
    /// GException is a POstSharp aspect that will log exceptions at the point they are riased.
    /// This allows for logging of handled as well as unhandled exceptions.
    /// </summary>
    [Serializable]
    public sealed class GException : GAspectBase
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

        private string _caption;

        public override void CompileTimeInitialize(MethodBase method)
        {
            base.CompileTimeInitialize(method);
            _caption = "Exception Exit " + QualifiedMethodName;
        }

        /// <summary>
        /// Exceptions are logged as Warnings because they may be handled.  If not,
        /// Gibraltar's default handler will log the unhandled exception as an Error.
        /// </summary>
        public override void OnException(MethodExecutionEventArgs eventArgs)
        {
            // Get out fast if tracing is disabled
            if (!Enabled)
                return;

            string description = eventArgs.Exception.Message;
            Log.Write(LogMessageSeverity.Warning, "PostSharp", this, null, eventArgs.Exception, LogWriteMode.Queued,
                      null, "PostSharp.Exit", Indent(_caption), description);
        }
    }
}