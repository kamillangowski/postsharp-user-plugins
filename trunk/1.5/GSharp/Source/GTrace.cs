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
using Gibraltar.Agent;
using PostSharp.Laos;

#endregion

namespace Gibraltar.GSharp
{
    /// <summary>
    /// GTrace is a PostSharp aspect used to trace entry and successful exit from methods.  It also measures method execution time.
    /// </summary>
    [Serializable]
    public sealed class GTrace : GAspectBase
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

        // This local variable ensures that we log exit if we logged entry
        [NonSerialized]
        private bool _tracingEnabled;

        private string _argumentFormat;
        private string _enterCaption;
        private string _exitCaption;
        private bool _hasArguments;
        private bool _isVoid;
        [NonSerialized]
        private Stopwatch _stopwatch;

        /// <summary>
        /// This method pre-computes as much as possible at compile time to minimize runtime processing requirements.
        /// </summary>
        public override void CompileTimeInitialize(MethodBase method)
        {
            base.CompileTimeInitialize(method);

            _enterCaption = "==> " + QualifiedMethodName;
            _exitCaption = "<== " + QualifiedMethodName;

            // This block of code is all about pre-calculating a format string containing the names
            // of each method argument.
            MethodInfo methodInfo = method as MethodInfo;
            _isVoid = methodInfo == null || methodInfo.ReturnType == typeof (void);
            ParameterInfo[] parameters = method.GetParameters();
            _hasArguments = parameters != null && parameters.Length > 0;
            if (_hasArguments)
            {
                _argumentFormat = "";
                int i = 0;
                // ReSharper disable PossibleNullReferenceException
                foreach (ParameterInfo info in parameters)
                    // ReSharper restore PossibleNullReferenceException
                {
                    _argumentFormat += string.Format("{0} = {{{1}}}\n", info.Name, i++);
                }
            }
        }

        public override void RuntimeInitialize(MethodBase method)
        {
            base.RuntimeInitialize(method);
            if (_stopwatch == null)
                _stopwatch = new Stopwatch();
        }

        /// <summary>
        /// This method is called each time a tagged method is called.
        /// </summary>
        public override void OnEntry(MethodExecutionEventArgs eventArgs)
        {
            // Get out fast if tracing is disabled
            _tracingEnabled = Enabled;
            if (!_tracingEnabled)
                return;

            // If the method has arguments, log the value of each parameter
            string description = null;
            if (_hasArguments)
            {
                object[] array = eventArgs.GetReadOnlyArgumentArray();
                description = string.Format(_argumentFormat, array);
            }

            Log.Write(LogMessageSeverity.Verbose, "PostSharp", this, null, null, LogWriteMode.Queued, null,
                      "PostSharp.Enter", Indent(_enterCaption), description);
            Trace.Indent();

            // Last thing we do is get the timer so that we measure just the guts of this method
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        /// <summary>
        /// This method is called if the method returns normally.  It logs the e
        /// </summary>
        public override void OnSuccess(MethodExecutionEventArgs eventArgs)
        {
            // Get out fast if tracing is disabled
            if (!_tracingEnabled)
                return;

            _stopwatch.Stop();
            TimeSpan duration = _stopwatch.Elapsed;
            string durationText = "    ";
            if (duration.TotalMilliseconds < 10000)
                durationText += duration.TotalMilliseconds + " ms";
            else
                durationText += duration;

            string caption = Indent(_exitCaption, Trace.IndentLevel - 1);
            string returnText = _isVoid ? "" : "\nReturns: " + eventArgs.ReturnValue;
            string description = "Duration: " + durationText + returnText;

            Log.Write(LogMessageSeverity.Verbose, "PostSharp", this, null, null, LogWriteMode.Queued, null,
                      "PostSharp.Exit", caption, description);
        }

        public override void OnExit(MethodExecutionEventArgs eventArgs)
        {
            // Get out fast if tracing is disabled
            if (!_tracingEnabled)
                return;

            Trace.Unindent();
        }
    }
}