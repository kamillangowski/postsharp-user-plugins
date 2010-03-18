/*

Copyright (c) 2008, Michal Dabrowski

All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of the Michal Dabrowski nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

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

using PostSharp.Extensibility;

namespace Log4PostSharp
{
  /// <summary>
  /// Indicates that each time a method is entered or left this fact will be written in the log.
  /// </summary>
  /// <remarks>
  /// <para>Logging can occur at any of the three stages of the method execution:
  /// <list type="bullet">
  /// <item>Before the control flow enters the method,</item>
  /// <item>After the control flow exits the method after the method code is successfully executed,</item>
  /// <item>After the control flow exits the method because its execution is interrupted by an uncaught exception.</item>
  /// </list>
  /// Attribute defines properties for adjusting logging behavior (message severity and message text) for all these cases. 
  /// Setting message severity (log level) to <see cref="LogLevel.None"/> effectively disables logging for the specified case (i.e.
  /// respective logging code is not injected).</para>
  /// <para>Logging message can contain placeholders which are expanded to actual values before the message 
  /// is logged. Some of these values are already known during weaving, but others may vary between
  /// calls to the method and therefore can be determined only at run-time. Using the former ones
  /// has no impact on the performance of the injected code (i.e. performance is exactly same as if
  /// no placeholders were used), the latter, however, requires the Log4PostSharp to inject different,
  /// slower code.</para>
  /// <para>Following table lists the placeholders which are expanded when the method is woven:</para>
  /// <list type="table">
  /// <listheader><term>Placeholder</term><description>Action</description></listheader>
  /// <item><term>{signature}</term><description>Expanded to method signature (not including namespaces of parameter types or return value type).</description></item>
  /// </list>
  /// <para>Following table lists the placeholders which are expanded at run-time and therefore their appearance
  /// causes the Log4PostSharp to inject slower code:</para>
  /// <list type="table">
  /// <listheader><term>Placeholder</term><description>Action</description></listheader>
  /// <item><term>{@<i>parameter_name</i>}</term><description>Expanded to the value of the specified parameter of the method.</description></item>
  /// <item><term>{paramvalues}</term><description>Expanded to the comma-separated list of values of all parameters of the method. Value of every parameter is surrounded by quote-signs.</description></item>
  /// <item><term>{returnvalue}</term><description>Expanded to the value that the method returns. For methods that return no value, <see langword="null"/> is used.</description></item>
  /// </list>
  /// <para>Performance-wise, it does not make much of difference how many occurences of the "heavy" placeholders appear in a single
  /// message. What matters is that these are used at all. Also, messages for the same method are treated
  /// separately - using the "heavy" placeholders for the method entry message has no impact on the performance of
  /// the code injected for the exit or exception messages.</para>
  /// <para>Because of some log4net API limitations, the "heavy" placeholders cannot be used in the <see cref="ExceptionText"/>.
  /// Code will fail to weave if this rule is broken.</para>
  /// </remarks>
  [AttributeUsage(
    AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Module | AttributeTargets.Struct,
    AllowMultiple = true,
    Inherited = false)]
  [MulticastAttributeUsage(
  MulticastTargets.InstanceConstructor | MulticastTargets.StaticConstructor | MulticastTargets.Method,
    AllowMultiple = true)]
  [Serializable]
  [RequirePostSharp("Log4PostSharp", "Log4PostSharp.Weaver.LogTask")]
  public sealed class LogAttribute : MulticastAttribute
  {
    #region Private Fields

    /// <summary>
    /// Level of messages logged when a method is entered.
    /// </summary>
    private LogLevel entryLevel = LogLevel.Debug;

    /// <summary>
    /// Message to log when a method is entered.
    /// </summary>
    private string entryText = "{method}({inparamvalues})";

    /// <summary>
    /// Level of messages logged when a method is exited normally (i.e. without throwing exception).
    /// </summary>
    private LogLevel exitLevel = LogLevel.None;

    /// <summary>
    /// Message to log when a method is exited normally (i.e. without throwing exception).
    /// </summary>
    private string exitText = "{method} = {returnvalue} ({outparamvalues})";

    /// <summary>
    /// Level of messages logged when an exception is thrown from a method.
    /// </summary>
    private LogLevel exceptionLevel = LogLevel.Error;

    /// <summary>
    /// Message to log when an exception is thrown from a method.
    /// </summary>
    private string exceptionText = "{method} failed with exception";

    /// <summary>
    /// Priority of this aspect.
    /// </summary>
    private int aspectPriority = 0;

    /// <summary>
    /// Underlying field for the <see cref="IncludeCompilerGeneratedCode"/> property.
    /// </summary>
    private bool includeCompilerGeneratedCode = false;

    /// <summary>
    /// Underlying field for the <see cref="IncludeParamName"/> property.
    /// </summary>
    private bool includeParamName = true;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets the level of messages logged when a method is entered.
    /// </summary>
    /// <remarks>
    /// <para>Default value of this proprerty is <see cref="LogLevel.None"/>.</para>
    /// </remarks>
    public LogLevel EntryLevel
    {
      get { return this.entryLevel; }
      set { this.entryLevel = value; }
    }

    /// <summary>
    /// Gets or sets the message to log when a method is entered.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>Default value of this proprerty is the following string: "Entering method: {signature}.".</para>
    /// <para>Please refer to the class documentation for more information.</para>
    /// </remarks>
    public string EntryText
    {
      get { return this.entryText; }
      set
      {
        if (value == null)
        {
          throw new ArgumentNullException("value");
        }

        this.entryText = value;
      }
    }

    /// <summary>
    /// Gets or sets the level of messages logged when a method is exited normally (i.e. without throwing exception).
    /// </summary>
    /// <remarks>
    /// <para>Default value of this proprerty is <see cref="LogLevel.None"/>.</para>
    /// </remarks>
    public LogLevel ExitLevel
    {
      get { return this.exitLevel; }
      set { this.exitLevel = value; }
    }

    /// <summary>
    /// Gets or sets the message to log when a method is exited normally (i.e. without throwing exception).
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>Default value of this proprerty is the following string: "Exiting method: {signature}.".</para>
    /// <para>Please refer to the class documentation for more information.</para>
    /// </remarks>
    public string ExitText
    {
      get { return this.exitText; }
      set
      {
        if (value == null)
        {
          throw new ArgumentNullException("value");
        }

        this.exitText = value;
      }
    }

    /// <summary>
    /// Gets or sets the level of messages logged when an exception is thrown from a method.
    /// </summary>
    /// <remarks>
    /// <para>Default value of this proprerty is <see cref="LogLevel.None"/>.</para>
    /// </remarks>
    public LogLevel ExceptionLevel
    {
      get { return this.exceptionLevel; }
      set { this.exceptionLevel = value; }
    }

    /// <summary>
    /// Gets or sets the message to log when an exception is thrown from a method.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>Default value of this proprerty is the following string: "Exception thrown from method: {signature}.".</para>
    /// <para>Please refer to the class documentation for more information.</para>
    /// </remarks>
    public string ExceptionText
    {
      get { return this.exceptionText; }
      set
      {
        if (value == null)
        {
          throw new ArgumentNullException("value");
        }

        this.exceptionText = value;
      }
    }

    /// <summary>
    /// Gets or sets the priority of this aspect.
    /// </summary>
    public int AspectPriority
    {
      get { return this.aspectPriority; }
      set { this.aspectPriority = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the logging code is injected into the compiler-generated methods.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to inject the code into the compiler-generated methods
    /// or <see langword="false"/> otherwise.
    /// </value>
    /// <remarks>
    /// <para>Default value for this property is <see langword="false"/>.</para>
    /// </remarks>
    public bool IncludeCompilerGeneratedCode
    {
      get { return this.includeCompilerGeneratedCode; }
      set { this.includeCompilerGeneratedCode = value; }
    }

    /// <summary>
    /// Indicates whether parameters are logged with their respective parameter names.
    /// </summary>
    /// <remarks>
    /// The default is <c>true</c>.
    /// </remarks>
    public bool IncludeParamName
    {
      get { return includeParamName; }
      set { includeParamName = value; }
    }

    #endregion

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="LogAttribute"/> class.
    /// </summary>
    public LogAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogAttribute"/> class with the specified entry
    /// message details.
    /// </summary>
    /// <param name="entryLevel">Level of the message that will be logged when a method is entered.</param>
    /// <param name="entryText">Message to log when a method is entered.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entryText"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>This constructor also sets <see cref="ExceptionLevel"/> to <see cref="LogLevel.Error"/>.</para>
    /// <para>Please refer to the class documentation for more information.</para>
    /// </remarks>
    public LogAttribute(LogLevel entryLevel, string entryText)
    {
      if (entryText == null)
      {
        throw new ArgumentNullException("entryText");
      }

      this.entryLevel = entryLevel;
      this.entryText = entryText;
      this.exceptionLevel = LogLevel.Error;
    }

    #endregion
  }
}