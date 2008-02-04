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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

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
		/// System.Boolean type.
		/// </summary>
		private ITypeSignature boolType;

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

		/// <summary>
		/// Collection of per type logging information.
		/// </summary>
		private readonly Dictionary<TypeDefDeclaration, PerTypeLoggingData> perTypeLoggingDatas = new Dictionary<TypeDefDeclaration, PerTypeLoggingData>();

		#endregion

		#region Private Methods

		/// <summary>
		/// Creates a private static readonly field.
		/// </summary>
		/// <param name="name">Name of the field.</param>
		/// <param name="type">Type of the field.</param>
		/// <returns>Private static readonly field of the specified type.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
		private FieldDefDeclaration CreateField(string name, ITypeSignature type) {
			if (name == null) {
				throw new ArgumentNullException("name");
			}
			if (type == null) {
				throw new ArgumentNullException("type");
			}

			FieldDefDeclaration field = new FieldDefDeclaration();
			field.Attributes = FieldAttributes.InitOnly | FieldAttributes.Private | FieldAttributes.Static;
			field.Name = name;
			field.FieldType = type;
			return field;
		}

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
			IMethod isLoggingEnabledGetter = module.FindMethod(typeof (ILog).GetProperty(isLoggingEnabledGetterName).GetGetMethod(), BindingOptions.Default);
			IMethod logStringMethod = module.FindMethod(typeof (ILog).GetMethod(logStringMethodName, new Type[] {typeof (string)}), BindingOptions.Default);
			IMethod logStringExceptionMethod = module.FindMethod(typeof (ILog).GetMethod(logStringExceptionMethodName, new Type[] {typeof (string), typeof (Exception)}), BindingOptions.Default);

			return new LogLevelSupportItem(isLoggingEnabledGetter, logStringMethod, logStringExceptionMethod);
		}

		#endregion

		#region Internal Properties

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

		internal PerTypeLoggingData GetPerTypeLoggingData(TypeDefDeclaration type) {
			return this.perTypeLoggingDatas[type];
		}

		#endregion

		#region Protected Methods

		protected override void Initialize() {
			// Target module.
			ModuleDeclaration module = this.Project.Module;

			// Prepare types and methods. They will be used later by advices.
			this.boolType = module.FindType(typeof (bool), BindingOptions.Default);
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
					// Type whose constructor is being woven.
					TypeDefDeclaration wovenType = methodDef.DeclaringType;

					// Do not weave interface.
					if ((wovenType.Attributes & TypeAttributes.Interface) != TypeAttributes.Interface) {
						// Constructs a custom attribute instance. 
						LogAttribute attribute = (LogAttribute) CustomAttributeHelper.ConstructRuntimeObject(customAttributeEnumerator.Current.Value, this.Project.Module);

						// Build an advice based on this custom attribute. 
						LogAdvice advice = new LogAdvice(this, attribute);

						// Join point kinds that are used by respective logging code.
						JoinPointKinds enterKinds = (attribute.EntryLevel != LogLevel.None) ? JoinPointKinds.BeforeMethodBody : 0;
						JoinPointKinds exitKinds = (attribute.ExitLevel != LogLevel.None) ? JoinPointKinds.AfterMethodBodySuccess : 0;
						JoinPointKinds exceptionKinds = (attribute.ExceptionLevel != LogLevel.None) ? JoinPointKinds.AfterMethodBodyException : 0;
						// Sum of all required join point kinds;
						JoinPointKinds effectiveKinds = enterKinds | exitKinds | exceptionKinds;

						// Ensure there is at least one join point the logging advice applies to.
						if (effectiveKinds != 0) {
							if (!this.perTypeLoggingDatas.ContainsKey(wovenType)) {
								this.perTypeLoggingDatas.Add(wovenType, new PerTypeLoggingData());

								// Logging data for the woven type.
								PerTypeLoggingData perTypeLoggingData = this.perTypeLoggingDatas[wovenType];

								// Field where ILog instance is stored.
								FieldDefDeclaration logField = this.CreateField("~log4PostSharp~log", this.ilogType);
								wovenType.Fields.Add(logField);
								perTypeLoggingData.Log = logField;

								foreach (KeyValuePair<LogLevel, LogLevelSupportItem> levelsAndItems in this.levelSupportItems) {
									LogLevel logLevel = levelsAndItems.Key;

									string isLoggingEnabledFieldName = string.Format(CultureInfo.InvariantCulture, "~log4PostSharp~is{0}Enabled", logLevel);
									FieldDefDeclaration isLoggingEnabledField = this.CreateField(isLoggingEnabledFieldName, this.boolType);
									wovenType.Fields.Add(isLoggingEnabledField);
									perTypeLoggingData.IsLoggingEnabledField[logLevel] = isLoggingEnabledField;
								}

								codeWeaver.AddTypeLevelAdvice(new LogInitializeAdvice(this),
								                              JoinPointKinds.BeforeStaticConstructor,
															  new Singleton<TypeDefDeclaration>(wovenType));
							}

							codeWeaver.AddMethodLevelAdvice(advice,
							                                new Singleton<MethodDefDeclaration>(methodDef),
							                                effectiveKinds,
							                                null);
						}
					}
				}
			}
		}

		#endregion

		public IEnumerable GetLevelSupportItems() {
			return this.levelSupportItems.Values;
		}

		/// <summary>
		/// Produces MSIL code which adds logging fields and initializes them.
		/// </summary>
		private class LogInitializeAdvice : IAdvice {
			/// <summary>
			/// Task that owns this advice.
			/// </summary>
			private readonly LogTask parent;

			public LogInitializeAdvice(LogTask parent) {
				this.parent = parent;
			}

			#region IAdvice Members

			public int Priority {
				get { return int.MinValue; }
			}

			public bool RequiresWeave(WeavingContext context) {
				return true;
			}

			public void Weave(WeavingContext context, InstructionBlock block) {
				// Type whose constructor is being woven.
				TypeDefDeclaration wovenType = context.Method.DeclaringType;

				// Logging data for the woven type.
				PerTypeLoggingData perTypeLoggingData = this.parent.perTypeLoggingDatas[wovenType];

				InstructionSequence initializeSequence = context.Method.MethodBody.CreateInstructionSequence();

				block.AddInstructionSequence(initializeSequence, NodePosition.Before, null);

				context.InstructionWriter.AttachInstructionSequence(initializeSequence);
				context.InstructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);

				// Get the declaring type of the constructor.
				context.WeavingHelper.GetRuntimeType(GenericHelper.GetTypeCanonicalGenericInstance(wovenType), context.InstructionWriter);
				// Stack: type.
				// Get the logger for the method_declaring_type.
				context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.parent.GetLoggerMethod);
				// Stack: logger.
				// Assign logger to the log variable.
				context.InstructionWriter.EmitInstructionField(OpCodeNumber.Stsfld, GenericHelper.GetFieldCanonicalGenericInstance(perTypeLoggingData.Log));
				// Stack: .

				foreach (KeyValuePair<LogLevel, LogLevelSupportItem> levelsAndItems in this.parent.levelSupportItems) {
					LogLevel logLevel = levelsAndItems.Key;
					LogLevelSupportItem logLevelSupportItem = levelsAndItems.Value;

					// Check if the logger has debug output enabled.
					context.InstructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, GenericHelper.GetFieldCanonicalGenericInstance(perTypeLoggingData.Log));
					context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Callvirt, logLevelSupportItem.IsLoggingEnabledGetter);
					// Stack: isDebugEnabled.
					// Assign isDebugEnabled to the appropriate field.
					context.InstructionWriter.EmitInstructionField(OpCodeNumber.Stsfld, GenericHelper.GetFieldCanonicalGenericInstance(perTypeLoggingData.IsLoggingEnabledField[logLevel]));
					// Stack: .
				}

				context.InstructionWriter.DetachInstructionSequence();
			}

			#endregion
		}
	}
}