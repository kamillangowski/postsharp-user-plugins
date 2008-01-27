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

namespace Log4PostSharp {
	/// <summary>
	/// Indicates that each time a method is entered or left this fact will be written in the log.
	/// </summary>
	[AttributeUsage(
		AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Module | AttributeTargets.Struct, 
		AllowMultiple = true, 
		Inherited = false)]
	[MulticastAttributeUsage(
		MulticastTargets.Constructor | MulticastTargets.Method, 
		AllowMultiple = true)]
	[Serializable]
	public sealed class LogAttribute : MulticastAttribute, IRequirePostSharp {
		#region Private Fields

		/// <summary>
		/// Level of messages logged when a method is entered.
		/// </summary>
		private LogLevel entryLevel = LogLevel.None;

		/// <summary>
		/// Message to log when a method is entered.
		/// </summary>
		private string entryText = "Entering method: {signature}.";

		/// <summary>
		/// Level of messages logged when a method is exited normally (i.e. without throwing exception).
		/// </summary>
		private LogLevel exitLevel = LogLevel.None;

		/// <summary>
		/// Message to log when a method is exited normally (i.e. without throwing exception).
		/// </summary>
		private string exitText = "Exiting method: {signature}.";

		/// <summary>
		/// Level of messages logged when an exception is thrown from a method.
		/// </summary>
		private LogLevel exceptionLevel = LogLevel.None;

		/// <summary>
		/// Message to log when an exception is thrown from a method.
		/// </summary>
		private string exceptionText = "Exception thrown from method: {signature}.";

		/// <summary>
		/// Priority of this aspect.
		/// </summary>
		private int aspectPriority = 0;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the level of messages logged when a method is entered.
		/// </summary>
		/// <remarks>
		/// <para>Default value of this proprerty is <see cref="LogLevel.None"/>.</para>
		/// </remarks>
		public LogLevel EntryLevel {
			get { return this.entryLevel; }
			set { this.entryLevel = value; }
		}

		/// <summary>
		/// Gets or sets the message to log when a method is entered.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>Default value of this proprerty is the following string: "Entering method: {signature}.".</para>
		/// <para>Following placeholders are supported and will be expanded during weaving:</para>
		/// <list type="table">
		/// <listheader><term>Placeholder</term><description>Action</description></listheader>
		/// <item><term>{signature}</term><description>Expanded to method signature (not including namespaces of parameter types or return value type).</description></item>
		/// </list>
		/// </remarks>
		public string EntryText {
			get { return this.entryText; }
			set {
				if (value == null) {
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
		public LogLevel ExitLevel {
			get { return this.exitLevel; }
			set { this.exitLevel = value; }
		}

		/// <summary>
		/// Gets or sets the message to log when a method is exited normally (i.e. without throwing exception).
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>Default value of this proprerty is the following string: "Exiting method: {signature}.".</para>
		/// <para>Following placeholders are supported and will be expanded during weaving:</para>
		/// <list type="table">
		/// <listheader><term>Placeholder</term><description>Action</description></listheader>
		/// <item><term>{signature}</term><description>Expanded to method signature (not including namespaces of parameter types or return value type).</description></item>
		/// </list>
		/// </remarks>
		public string ExitText {
			get { return this.exitText; }
			set {
				if (value == null) {
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
		public LogLevel ExceptionLevel {
			get { return this.exceptionLevel; }
			set { this.exceptionLevel = value; }
		}

		/// <summary>
		/// Gets or sets the message to log when an exception is thrown from a method.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>Default value of this proprerty is the following string: "Exception thrown from method: {signature}.".</para>
		/// <para>Full contents of the exception are written to the log regardless of the value of this
		/// property (provided only that <see cref="ExceptionLevel"/> is other than <see cref="LogLevel.None"/>).</para>
		/// <para>Following placeholders are supported and will be expanded during weaving:</para>
		/// <list type="table">
		/// <listheader><term>Placeholder</term><description>Action</description></listheader>
		/// <item><term>{signature}</term><description>Expanded to method signature (not including namespaces of parameter types or return value type).</description></item>
		/// </list>
		/// </remarks>
		public string ExceptionText {
			get { return this.exceptionText; }
			set {
				if (value == null) {
					throw new ArgumentNullException("value");
				}
				
				this.exceptionText = value;
			}
		}

		/// <summary>
		/// Gets or sets the priority of this aspect.
		/// </summary>
		public int AspectPriority {
			get { return this.aspectPriority; }
			set { this.aspectPriority = value; }
		}

		#endregion

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="LogAttribute"/> class.
		/// </summary>
		public LogAttribute() {
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
		/// </remarks>
		public LogAttribute(LogLevel entryLevel, string entryText) {
			if (entryText == null) {
				throw new ArgumentNullException("entryText");
			}

			this.entryLevel = entryLevel;
			this.entryText = entryText;
			this.exceptionLevel = LogLevel.Error;
		}

		#endregion

		#region IRequirePostSharp Members

		PostSharpRequirements IRequirePostSharp.GetPostSharpRequirements() {
			PostSharpRequirements requirements = new PostSharpRequirements();
			requirements.PlugIns.Add("Log4PostSharp");
			requirements.Tasks.Add("Log4PostSharp.Weaver.LogTask");
			return requirements;
		}

		#endregion
	}
}