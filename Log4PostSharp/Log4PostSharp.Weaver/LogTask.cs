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
using System.Collections.Generic;
using System.Globalization;

using log4net;

using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;

namespace Log4PostSharp.Weaver {
	public class LogTask : Task, IAdviceProvider {
		#region Private Fields

		/// <summary>
		/// System.Type.GetTypeFromHandle(System.RuntimeTypeHandle) method.
		/// </summary>
		private IMethod getTypeFromHandleMethod;

		/// <summary>
		/// log4net.LogManager.GetLogger(System.Type) method.
		/// </summary>
		private IMethod getLoggerMethod;

		/// <summary>
		/// log4net.ILog type.
		/// </summary>
		private ITypeSignature ilogType;

		/// <summary>
		/// Collection of support items for different log levels.
		/// </summary>
		private readonly Dictionary<LogLevel, LogLevelSupportItem> levelSupportItems = new Dictionary<LogLevel, LogLevelSupportItem>();

		#endregion

		#region Private Methods

		/// <summary>
		/// Creates <see cref="LogLevelSupportItem"/> for the specified logging level.
		/// </summary>
		/// <param name="memberNamePart">"Debug", "Info", "Warn", "Error" or "Fatal" depending on the log level the item is to be created for.</param>
		/// <returns><see cref="LogLevelSupportItem"/> for the specified level.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="memberNamePart"/> is <see langword="null"/>.</exception>
		private LogLevelSupportItem CreateSupportItem(string memberNamePart) {
			if (memberNamePart == null) {
				throw new ArgumentNullException("memberNamePart");
			}

			// Target module.
			ModuleDeclaration module = this.Project.Module;

			string isLoggingEnabledGetterName = string.Format(CultureInfo.InvariantCulture, "Is{0}Enabled", memberNamePart);
			string logStringMethodName = memberNamePart;
			string logStringExceptionMethodName = memberNamePart;
			IMethod isLoggingEnabledGetter = module.FindMethod(typeof(ILog).GetProperty(isLoggingEnabledGetterName).GetGetMethod(), BindingOptions.Default);
			IMethod logStringMethod = module.FindMethod(typeof(ILog).GetMethod(logStringMethodName, new Type[] { typeof(string) }), BindingOptions.Default);
			IMethod logStringExceptionMethod = module.FindMethod(typeof(ILog).GetMethod(logStringExceptionMethodName, new Type[] { typeof(string), typeof(Exception) }), BindingOptions.Default);

			return new LogLevelSupportItem(isLoggingEnabledGetter, logStringMethod, logStringExceptionMethod);
		}

		#endregion

		#region Internal Properties

		/// <summary>
		/// Gets the System.Type.GetTypeFromHandle(System.RuntimeTypeHandle) method.
		/// </summary>
		public IMethod GetTypeFromHandleMethod {
			get { return this.getTypeFromHandleMethod; }
		}

		/// <summary>
		/// Gets the log4net.LogManager.GetLogger(System.Type) method.
		/// </summary>
		public IMethod GetLoggerMethod {
			get { return this.getLoggerMethod; }
		}

		/// <summary>
		/// Gets the log4net.ILog type.
		/// </summary>
		public ITypeSignature IlogType {
			get { return this.ilogType; }
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// Gets <see cref="LogLevelSupportItem"/> for the specified level.
		/// </summary>
		/// <param name="level">Level to get the support item for.</param>
		/// <returns>Support item for the level.</returns>
		internal LogLevelSupportItem GetSupportItem(LogLevel level) {
			return this.levelSupportItems[level];
		}

		#endregion

		#region Protected Methods

		protected override void Initialize() {
			// Target module.
			ModuleDeclaration module = this.Project.Module;

			// Prepare types and methods. They will be used later by advices.
			this.getTypeFromHandleMethod = module.FindMethod(typeof (Type).GetMethod("GetTypeFromHandle", new Type[] {typeof (RuntimeTypeHandle)}), BindingOptions.Default);
			this.getLoggerMethod = module.FindMethod(typeof (LogManager).GetMethod("GetLogger", new Type[] {typeof (Type)}), BindingOptions.Default);
			this.ilogType = module.FindType(typeof (ILog), BindingOptions.Default);

			// Prepare level support items for all levels.
			this.levelSupportItems[LogLevel.Debug] = this.CreateSupportItem("Debug");
			this.levelSupportItems[LogLevel.Info] = this.CreateSupportItem("Info");
			this.levelSupportItems[LogLevel.Warn] = this.CreateSupportItem("Warn");
			this.levelSupportItems[LogLevel.Error] = this.CreateSupportItem("Error");
			this.levelSupportItems[LogLevel.Fatal] = this.CreateSupportItem("Fatal");
		}

		#endregion

		#region IAdviceProvider Members

		public void ProvideAdvices(PostSharp.CodeWeaver.Weaver codeWeaver) {
			// Gets the dictionary of custom attributes.
			CustomAttributeDictionaryTask customAttributeDictionaryTask = CustomAttributeDictionaryTask.GetTask(this.Project);

			// Requests an enumerator of all instances of the LogAttribute.
			IEnumerator<ICustomAttributeInstance> customAttributeEnumerator = customAttributeDictionaryTask.GetCustomAttributesEnumerator(typeof (LogAttribute), false);

			// For each instance of the LogAttribute. 
			while (customAttributeEnumerator.MoveNext()) {
				// Gets the method to which it applies. 
				MethodDefDeclaration methodDef = customAttributeEnumerator.Current.TargetElement as MethodDefDeclaration;
				if (methodDef != null) {
					// Constructs a custom attribute instance. 
					LogAttribute attribute = (LogAttribute) CustomAttributeHelper.ConstructRuntimeObject(customAttributeEnumerator.Current.Value, this.Project.Module);

					// Build an advice based on this custom attribute. 
					LogAdvice advice = new LogAdvice(this, attribute);

					codeWeaver.AddMethodLevelAdvice(advice,
					                                new Singleton<MethodDefDeclaration>(methodDef),
					                                JoinPointKinds.BeforeMethodBody | JoinPointKinds.AfterMethodBodySuccess | JoinPointKinds.AfterMethodBodyException,
					                                null);
				}
			}
		}

		#endregion
	}
}