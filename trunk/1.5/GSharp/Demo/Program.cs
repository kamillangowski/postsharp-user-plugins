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
using System.Windows.Forms;

#endregion

namespace GSharpDemo
{
    /// <summary>
    /// A sample application using dynamic binding to Gibraltar.Agent.dll via app.config.
    /// </summary>
    /// <remarks><para>
    /// This example is compiled without direct reference to Gibraltar, only adding Gibraltar's TraceListener
    /// (Gibraltar.Agent.LogListener) in the trace section of the system.diagnostics group in the app.config file to
    /// connect the Agent through the use of built-in Trace logging.  This allows the Gibraltar Agent to be attached to
    /// an application which is already using Trace or other logging systems without recompiling your application.
    /// </para>
    /// <para>
    /// This example shows a typical winforms Program.Main() with a few recommended calls to make sure the
    /// Gibraltar Agent can perform at its best.
    /// </para>
    /// <para>
    /// Also see the static binding sample application as an alternative approach showing some of the features of the
    /// Gibraltar API.</para></remarks>
    internal static class Program
    {
        public static TimeSpanCollection TimeSpans = new TimeSpanCollection();

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Fire off a message so Trace will scan the app.config (see also) and add Gibraltar's TraceListener.
            // This ensures that the Gibraltar Agent is loaded and activated, scanning its own part of app.config.
            // Without a message at this point, the Agent would only get loaded when a message is logged some time later,
            // and some of the Agent's automatic features would not be able to function at their best for you.

            Trace.TraceInformation("Application starting.");

            // Nothing else is needed to activate exception handling with Gibraltar on most winforms apps, 
            // as soon as the first line of logging returns above it's active.

            Application.Run(new MainApp());

            Trace.TraceInformation("Application exiting."); // Just for completeness.
            Trace.Close(); // It's always a good idea to call Trace.Close when you're exiting the application.  This ensures that every listener has the chance to shutdown cleanly.
        }
    }
}