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
using System.Text;

using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.ModuleWriter;

namespace Log4PostSharp.Weaver {
	/// <summary>
	/// Produces MSIL code for logging.
	/// </summary>
	public class LogAdvice : IAdvice {
		#region Private Fields

		/// <summary>
		/// Task this advice is attached to.
		/// </summary>
		private readonly LogTask parent;

		/// <summary>
		/// Attribute this advice is attached to.
		/// </summary>
		private readonly LogAttribute attribute;

		#endregion

		#region Private Methods

		/// <summary>
		/// Emits the MSIL code that jumps to the specified label if logging is disabled.
		/// </summary>
		/// <param name="emitter">IL emitter.</param>
		/// <param name="logLevelSupportItem">Item for the logging level.</param>
		/// <param name="perTypeLoggingData">Data for the type being woven.</param>
		/// <param name="afterLoggingSequence">Sequence to jump to if logging is disabled.</param>
		/// <exception cref="ArgumentNullException"><paramref name="emitter"/>, <paramref name="logLevelSupportItem"/>, <paramref name="perTypeLoggingData"/> or <paramref name="afterLoggingSequence"/> is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>Code emitted by this method makes no assumptions on the state of the evaluation stack 
		/// and it leaves the stack unmodified.</para>
		/// </remarks>
		private static void EmitLoggingEnabledCheck(InstructionEmitter emitter, LogLevelSupportItem logLevelSupportItem, PerTypeLoggingData perTypeLoggingData, InstructionSequence afterLoggingSequence) {
			if (emitter == null) {
				throw new ArgumentNullException("emitter");
			}
		    if (logLevelSupportItem == null) {
		        throw new ArgumentNullException("logLevelSupportItem");
		    }
		    if (perTypeLoggingData == null) {
		        throw new ArgumentNullException("perTypeLoggingData");
		    }
		    if (afterLoggingSequence == null) {
				throw new ArgumentNullException("afterLoggingSequence");
			}

            emitter.EmitInstructionField(OpCodeNumber.Ldsfld, GenericHelper.GetFieldCanonicalGenericInstance(perTypeLoggingData.Log));
            emitter.EmitInstructionMethod(OpCodeNumber.Callvirt,  logLevelSupportItem.IsLoggingEnabledGetter);
			emitter.EmitInstruction(OpCodeNumber.Ldc_I4_0);
			emitter.EmitInstruction(OpCodeNumber.Ceq);
			emitter.EmitBranchingInstruction(OpCodeNumber.Brtrue_S, afterLoggingSequence);
		}

		/// <summary>
		/// Emits the MSIL that calls the logging method with the specified message.
		/// </summary>
		/// <param name="emitter">IL emitter.</param>
		/// <param name="log">Field that stores reference to the logger.</param>
		/// <param name="method">Logging method to use. The method must return no value and take 1 parameter of type <see cref="string"/>.</param>
		/// <param name="message">Message to log.</param>
		/// <exception cref="ArgumentNullException"><paramref name="emitter"/>, <paramref name="log"/>, <paramref name="method"/> or <paramref name="message"/> is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>Code emitted by this method makes no assumptions on the state of the evaluation stack 
		/// and it leaves the stack unmodified.</para>
		/// </remarks>
		private static void EmitLogString(InstructionEmitter emitter, FieldDefDeclaration log, IMethod method, string message) {
			if (emitter == null) {
				throw new ArgumentNullException("emitter");
			}
			if (log == null) {
				throw new ArgumentNullException("log");
			}
			if (method == null) {
				throw new ArgumentNullException("method");
			}
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			emitter.EmitInstructionField(OpCodeNumber.Ldsfld, GenericHelper.GetFieldCanonicalGenericInstance(log));
			emitter.EmitInstructionString(OpCodeNumber.Ldstr, message);
			emitter.EmitInstructionMethod(OpCodeNumber.Callvirt, method);
		}

		/// <summary>
		/// Emits the MSIL that calls the logging method with the specified message and exception.
		/// </summary>
		/// <param name="emitter">IL emitter.</param>
		/// <param name="log">Field that stores reference to the logger.</param>
		/// <param name="method">Logging method to use. The method must return no value and must take 2 parameters of types <see cref="string"/> and <see cref="Exception"/>.</param>
		/// <param name="message">Message to log.</param>
		/// <param name="exception">Local variable where reference to the exception is stored.</param>
		/// <exception cref="ArgumentNullException"><paramref name="emitter"/>, <paramref name="log"/>, <paramref name="method"/>, <paramref name="message"/> or <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>Code emitted by this method makes no assumptions on the state of the evaluation stack 
		/// and it leaves the stack unmodified.</para>
		/// </remarks>
		private static void EmitLogStringException(InstructionEmitter emitter, FieldDefDeclaration log, IMethod method, string message, LocalVariableSymbol exception) {
			if (emitter == null) {
				throw new ArgumentNullException("emitter");
			}
			if (log == null) {
				throw new ArgumentNullException("log");
			}
			if (method == null) {
				throw new ArgumentNullException("method");
			}
			if (message == null) {
				throw new ArgumentNullException("message");
			}
			if (exception == null) {
				throw new ArgumentNullException("exception");
			}

			emitter.EmitInstructionField(OpCodeNumber.Ldsfld, GenericHelper.GetFieldCanonicalGenericInstance(log));
			emitter.EmitInstructionString(OpCodeNumber.Ldstr, message);
			emitter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc_S, exception);
			emitter.EmitInstructionMethod(OpCodeNumber.Callvirt, method);
		}

		/// <summary>
		/// Emits the MSIL that calls the logging method with the specified format provider, message and arguments.
		/// </summary>
		/// <param name="emitter">IL emitter.</param>
		/// <param name="log">Field that stores reference to the logger.</param>
		/// <param name="method">Logging method to use. The method must return no value and must take 3 arguments of types <see cref="IFormatProvider"/>, <see cref="string"/> and <see cref="Array"/> of <see cref="object"/>s.</param>
		/// <param name="formatProviderGetter">Getter of the property that returns the <see cref="IFormatProvider"/> instance.</param>
		/// <param name="formatString">Format string for the log.</param>
		/// <param name="args">Variable storing reference to array of arguments for placeholders used in the format string.</param>
		/// <exception cref="ArgumentNullException"><paramref name="emitter"/>, <paramref name="log"/>, <paramref name="formatProviderGetter"/>, <paramref name="formatString"/> or <paramref name="args"/> is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>Code emitted by this method makes no assumptions on the state of the evaluation stack 
		/// and it leaves the stack unmodified.</para>
		/// </remarks>
		private static void EmitLogProviderStringArgs(InstructionEmitter emitter, FieldDefDeclaration log, IMethod method, IMethod formatProviderGetter, string formatString, LocalVariableSymbol args) {
			if (emitter == null) {
				throw new ArgumentNullException("emitter");
			}
			if (log == null) {
				throw new ArgumentNullException("log");
			}
			if (method == null) {
				throw new ArgumentNullException("method");
			}
			if (formatProviderGetter == null) {
				throw new ArgumentNullException("formatProviderGetter");
			}
			if (args == null) {
				throw new ArgumentNullException("args");
			}

			emitter.EmitInstructionField(OpCodeNumber.Ldsfld, GenericHelper.GetFieldCanonicalGenericInstance(log));
			emitter.EmitInstructionMethod(OpCodeNumber.Call, formatProviderGetter);
			emitter.EmitInstructionString(OpCodeNumber.Ldstr, formatString);
			emitter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, args);
			emitter.EmitInstructionMethod(OpCodeNumber.Callvirt, method);
		}

		/// <summary>
		/// Emits the MSIL that creates local variable and initializes it with array of objects representing the specified tokens.
		/// </summary>
		/// <param name="context">Context for the weaving.</param>
		/// <param name="block">Block where the code has to be injected.</param>
		/// <param name="nonStaticTokens">List of tokens that the object array is created for.</param>
		/// <returns>Local variable that stores the reference to the array.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="context"/>, <paramref name="block"/> or <paramref name="nonStaticTokens"/> is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>Code emitted by this method makes no assumptions on the state of the evaluation stack 
		/// and it leaves the stack unmodified.</para>
		/// </remarks>
		private LocalVariableSymbol EmitCreateFormatArgumentArray(WeavingContext context, InstructionBlock block, IList<IMessageToken> nonStaticTokens) {
			if (context == null) {
				throw new ArgumentNullException("context");
			}
			if (block == null) {
				throw new ArgumentNullException("block");
			}
			if (nonStaticTokens == null) {
				throw new ArgumentNullException("nonStaticTokens");
			}

			InstructionEmitter emitter = context.InstructionWriter;

			// Array that store arguments for the formatting.
			LocalVariableSymbol args = block.DefineLocalVariable(context.Method.Module.FindType(typeof(object[]), BindingOptions.Default), "~args~{0}");

			// Create the array for storing agruments for formatting (contains only dyncamic tokens).
			emitter.EmitInstructionInt32(OpCodeNumber.Ldc_I4, nonStaticTokens.Count);
			emitter.EmitInstructionType(OpCodeNumber.Newarr, this.parent.ObjectType);
			// Save the array into the local variable because it will be used multiple times.
			emitter.EmitInstructionLocalVariable(OpCodeNumber.Stloc, args);

			// Fill the array with the data.
			for (int index = 0; index < nonStaticTokens.Count; index++) {
				// Array to store the data in.
				emitter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, args);
				// Position in the array to store the data at.
				emitter.EmitInstructionInt32(OpCodeNumber.Ldc_I4, index);
				// Let the token generate the IL that pushes the argument onto the stack.
				nonStaticTokens[index].Emit(context);
				// Finally: store the generated object into the array at the given position.
				emitter.EmitInstruction(OpCodeNumber.Stelem_Ref);
			}

			return args;
		}

		/// <summary>
		/// Creates format string that is formed by direct copying static tokens and generating placeholders
		/// for dynamic tokens.
		/// </summary>
		/// <param name="messageTokens">List of tokens to create the format string for.</param>
		/// <param name="messageFormatString">Builder to build the format string with.</param>
		/// <param name="dynamicTokens">Collection where the method puts processed dynamic tokens. The tokens are added in the same order as their respective placeholders.</param>
		/// <exception cref="ArgumentNullException"><paramref name="messageTokens"/>, <paramref name="messageFormatString"/> or <paramref name="dynamicTokens"/> is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>Output of the method (the contents of <paramref name="messageFormatString"/>) can be passed
		/// to <see cref="string.Format(IFormatProvider,string,object[])"/> (or compatible) method. Also,
		/// an array created from the <paramref name="dynamicTokens"/> can be passed as the third parameter to the
		/// formatting method. Order of placeholders in the format string matches the order of the tokens
		/// in the <paramref name="dynamicTokens"/> list.</para>
		/// </remarks>
		private static void MakeFormatString(IEnumerable<IMessageToken> messageTokens, StringBuilder messageFormatString, ICollection<IMessageToken> dynamicTokens) {
			if (messageTokens == null) {
				throw new ArgumentNullException("messageTokens");
			}
			if (messageFormatString == null) {
				throw new ArgumentNullException("messageFormatString");
			}
			if (dynamicTokens == null) {
				throw new ArgumentNullException("dynamicTokens");
			}

			foreach (IMessageToken token in messageTokens) {
				if (token.IsStatic) {
					// Append the token text directly.
					messageFormatString.Append(token.Text);
				} else {
					// Append the placeholder for the token.
					messageFormatString.Append("{" + dynamicTokens.Count.ToString(CultureInfo.InvariantCulture) + "}");
					dynamicTokens.Add(token);
				}
			}
		}

		/// <summary>
		/// Emits the MSIL which checks if the logging is enabled and logs the message.
		/// </summary>
		/// <param name="context">Weaving context.</param>
		/// <param name="block">Block where the code has to be injected.</param>
		/// <param name="level">Level for the message.</param>
		/// <param name="template">Template that will be tokenized in order to get message text.</param>
		/// <exception cref="ArgumentNullException"><paramref name="context"/>, <paramref name="block"/> or <paramref name="template"/> is <see langword="null"/>.</exception>
		/// <exception cref="FormatException">Template is not valid.</exception>
		/// <remarks>
		/// <para>If the <paramref name="level"/> is set to <see cref="LogLevel.None"/>, the method emits no code.</para>
		/// </remarks>
		private void EmitCheckingAndTemplateLogging(WeavingContext context, InstructionBlock block, LogLevel level, string template) {
			if (context == null) {
				throw new ArgumentNullException("context");
			}
			if (block == null) {
				throw new ArgumentNullException("block");
			}
			if (template == null) {
				throw new ArgumentNullException("template");
			}

			if (level != LogLevel.None) {
				// Method being woven and the type the method is declared in.
				MethodDefDeclaration wovenMethod = context.Method;
				TypeDefDeclaration wovenType = wovenMethod.DeclaringType;

				// Objects that contain required methods, fields, etc.
				LogLevelSupportItem supportItem = this.parent.GetSupportItem(level);
				PerTypeLoggingData perTypeLoggingData = this.parent.GetPerTypeLoggingData(wovenType);

				// Get the tokens for the message template.
				StringBuilder messageFormatString = new StringBuilder();
				List<IMessageToken> nonStaticTokens = new List<IMessageToken>();
				List<IMessageToken> messageParts = TemplateParser.Tokenize(template, wovenMethod);
				MakeFormatString(messageParts, messageFormatString, nonStaticTokens);

				// Sequence that contains the logging check and the logging itself.
				InstructionSequence logEntrySequence = wovenMethod.MethodBody.CreateInstructionSequence();
				block.AddInstructionSequence(logEntrySequence, NodePosition.Before, null);

				// Sequence that follows the logging code.
				InstructionSequence afterLoggingSequence = wovenMethod.MethodBody.CreateInstructionSequence();
				block.AddInstructionSequence(afterLoggingSequence, NodePosition.After, logEntrySequence);

				// Check if logging is enabled and log the message.
				context.InstructionWriter.AttachInstructionSequence(logEntrySequence);
				context.InstructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
			    EmitLoggingEnabledCheck(context.InstructionWriter, supportItem, perTypeLoggingData, afterLoggingSequence);
				if (nonStaticTokens.Count == 0) {
					// There are no dynamic tokens, use the faster logging method.
					EmitLogString(context.InstructionWriter, perTypeLoggingData.Log, supportItem.LogStringMethod, messageFormatString.ToString());
				} else {
					// There are dynamic tokens, prepare log message at run-time.
					LocalVariableSymbol args = this.EmitCreateFormatArgumentArray(context, block, nonStaticTokens);
					EmitLogProviderStringArgs(context.InstructionWriter, perTypeLoggingData.Log, supportItem.LogCultureStringArgsMethod, this.parent.InvariantCultureGetter, messageFormatString.ToString(), args);
				}
				context.InstructionWriter.DetachInstructionSequence();

				// Logging is finished (or skipped), do nothing.
				context.InstructionWriter.AttachInstructionSequence(afterLoggingSequence);
				context.InstructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Nop);
				context.InstructionWriter.DetachInstructionSequence();
			}
		}

		private void WeaveEnter(WeavingContext context, InstructionBlock block) {
			LogLevel level = this.attribute.EntryLevel;
			string text = this.attribute.EntryText;

			this.EmitCheckingAndTemplateLogging(context, block, level, text);
		}

		private void WeaveSuccess(WeavingContext context, InstructionBlock block) {
			LogLevel level = this.attribute.ExitLevel;
			string text = this.attribute.ExitText;

			this.EmitCheckingAndTemplateLogging(context, block, level, text);
		}

		private void WeaveException(WeavingContext context, InstructionBlock block) {
			LogLevel level = this.attribute.ExceptionLevel;
			string text = this.attribute.ExceptionText;

			// Inject the logging code only if the logging is turned on.
			if (this.attribute.ExceptionLevel != LogLevel.None) {
				// Method being woven and the type the method is declared in.
				MethodDefDeclaration wovenMethod = context.Method;
				TypeDefDeclaration wovenType = wovenMethod.DeclaringType;

				// Objects that contain required methods, fields, etc.
				LogLevelSupportItem supportItem = this.parent.GetSupportItem(level);
				PerTypeLoggingData perTypeLoggingData = this.parent.GetPerTypeLoggingData(wovenType);

				// Get the tokens for the message template.
				StringBuilder messageFormatString = new StringBuilder();
				List<IMessageToken> nonStaticTokens = new List<IMessageToken>();
				List<IMessageToken> messageParts = TemplateParser.Tokenize(text, wovenMethod);
				MakeFormatString(messageParts, messageFormatString, nonStaticTokens);

				// As log4net does not provide an overload for the LogXXX() methods which would accept
				// both exception and array of arguments for a format string, disallow usage of dynamic
				// tokens in the template.
				if (nonStaticTokens.Count > 0) {
					throw new FormatException("Message for logging exception can contain only placeholders whose value can be expanded at weaving time.");
				}

				// Variable that stores the reference to the thrown exception.
				LocalVariableSymbol exception = block.DefineLocalVariable(context.Method.Module.FindType(typeof(Exception), BindingOptions.Default), "~ex~{0}");

				// Sequence that contains code that checks if the logging is enabled and logs the message.
				InstructionSequence logExceptionSequence = context.Method.MethodBody.CreateInstructionSequence();
				block.AddInstructionSequence(logExceptionSequence, NodePosition.Before, null);
				// Sequence that contains code that is executed after logging.
				InstructionSequence afterLoggingSequence = context.Method.MethodBody.CreateInstructionSequence();
				block.AddInstructionSequence(afterLoggingSequence, NodePosition.After, logExceptionSequence);

				// Emit code that checks if the logging is enabled and logs the message.
				context.InstructionWriter.AttachInstructionSequence(logExceptionSequence);
				context.InstructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
				context.InstructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Stloc_S, exception);
				EmitLoggingEnabledCheck(context.InstructionWriter, supportItem, perTypeLoggingData, afterLoggingSequence);
				EmitLogStringException(context.InstructionWriter, perTypeLoggingData.Log, supportItem.LogStringExceptionMethod, messageFormatString.ToString(), exception);
				context.InstructionWriter.DetachInstructionSequence();

				// After logging is finished (or skipped), rethrow the exception.
				context.InstructionWriter.AttachInstructionSequence(afterLoggingSequence);
				context.InstructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Rethrow);
				context.InstructionWriter.DetachInstructionSequence();
			} else {
				// Logging is turned off, just rethrow the exception.
				InstructionSequence rethrowSequence = context.Method.MethodBody.CreateInstructionSequence();
				block.AddInstructionSequence(rethrowSequence, NodePosition.Before, null);
				context.InstructionWriter.AttachInstructionSequence(rethrowSequence);
				context.InstructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Rethrow);
				context.InstructionWriter.DetachInstructionSequence();
			}
		}

		#endregion

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="LogAdvice"/> class.
		/// </summary>
		/// <param name="parent">Task that creates the advice.</param>
		/// <param name="attribute">Attribute the advice is associated with.</param>
		/// <exception cref="ArgumentNullException"><paramref name="parent"/> or <see cref="attribute"/> is <see langword="null"/>.</exception>
		public LogAdvice(LogTask parent, LogAttribute attribute) {
			if (parent == null) {
				throw new ArgumentNullException("parent");
			}
			if (attribute == null) {
				throw new ArgumentNullException("attribute");
			}

			this.parent = parent;
			this.attribute = attribute;
		}

		#endregion

		#region IAdvice Members

		public int Priority {
			get { return this.attribute.AspectPriority; }
		}

		public bool RequiresWeave(WeavingContext context) {
			return true;
		}

		public void Weave(WeavingContext context, InstructionBlock block) {
			switch (context.JoinPoint.JoinPointKind) {
				case JoinPointKinds.AfterMethodBodyException:
					this.WeaveException(context, block);
					break;
				case JoinPointKinds.AfterMethodBodySuccess:
					this.WeaveSuccess(context, block);
					break;
				case JoinPointKinds.BeforeMethodBody:
					this.WeaveEnter(context, block);
					break;
			}
		}

		#endregion
	}
}