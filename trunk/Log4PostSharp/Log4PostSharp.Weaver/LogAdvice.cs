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
using System.Globalization;

using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;

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
		/// Gets text representing the specified type.
		/// </summary>
		/// <param name="type">Type to get text representation for.</param>
		/// <returns>Text representing the <paramref name="type"/></returns>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		private static string GetTypeName(ITypeSignature type) {
			if (type == null) {
				throw new ArgumentNullException("type");
			}

			return type.GetSystemType(null, null).Name;
		}

		/// <summary>
		/// Gets text containing signature of the specified method.
		/// </summary>
		/// <param name="method">Method.</param>
		/// <returns>Signature of the specified method.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		private static string GetMethodSignature(MethodDefDeclaration method) {
			if (method == null) {
				throw new ArgumentNullException("method");
			}

			string returnType = GetTypeName(method.ReturnParameter.ParameterType);

			string[] parameterTypes = new string[method.Parameters.Count];
			for (int i = 0; i < parameterTypes.Length; i++) {
				parameterTypes[i] = GetTypeName(method.Parameters[i].ParameterType);
			}

			return string.Format(CultureInfo.InvariantCulture, "{0} {1}({2})", returnType, method.Name, string.Join(", ", parameterTypes));
		}

		/// <summary>
		/// Prepares the message to log (expands all placeholders).
		/// </summary>
		/// <param name="method">Method the log entry is created for.</param>
		/// <param name="format">Message text containing the placeholders.</param>
		/// <returns>Message with placeholders expanded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> or <paramref name="format"/> is <see langword="null"/>.</exception>
		private static string GetMessage(MethodDefDeclaration method, string format) {
			if (method == null) {
				throw new ArgumentNullException("method");
			}
			if (format == null) {
				throw new ArgumentNullException("format");
			}

			// Get the signature in case it is needed.
			string signature = GetMethodSignature(method);

			// Expand placeholders.
			string message = format.Replace("{signature}", signature);

			return message;
		}

		private void WeaveEnter(WeavingContext context, InstructionBlock block) {
			LogLevel entryLevel = this.attribute.EntryLevel;
			if (entryLevel != LogLevel.None) {
				string message = GetMessage(context.Method, this.attribute.EntryText);

				LogLevelSupportItem supportItem = this.parent.GetSupportItem(this.attribute.EntryLevel);
				TypeDefDeclaration wovenType = context.Method.DeclaringType;
				PerTypeLoggingData perTypeLoggingData = this.parent.GetPerTypeLoggingData(wovenType);

				InstructionSequence logEntrySequence = context.Method.MethodBody.CreateInstructionSequence();
				InstructionSequence afterLoggingSequence = context.Method.MethodBody.CreateInstructionSequence();

				block.AddInstructionSequence(logEntrySequence, NodePosition.Before, null);
				block.AddInstructionSequence(afterLoggingSequence, NodePosition.After, logEntrySequence);

				context.InstructionWriter.AttachInstructionSequence(logEntrySequence);
				context.InstructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);

				// Check if the logger has debug output enabled.
				context.InstructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, perTypeLoggingData.IsLoggingEnabledField[entryLevel]);
				// Stack: isDebugEnabled.
				// Compare the isDebugEnabled to 0.
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Ldc_I4_0);
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Ceq);
				// Stack: isDebugEnabled==0.
				// If isDebugEnabled==0 then skip logging.
				context.InstructionWriter.EmitBranchingInstruction(OpCodeNumber.Brtrue_S, afterLoggingSequence);
				// Stack: .
				// Call the logging method.
				context.InstructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, perTypeLoggingData.Log);
				context.InstructionWriter.EmitInstructionString(OpCodeNumber.Ldstr, message);
				context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Callvirt, supportItem.LogStringMethod);
				// Stack: .

				// Commit changes and detach the instruction sequence.
				context.InstructionWriter.DetachInstructionSequence();

				context.InstructionWriter.AttachInstructionSequence(afterLoggingSequence);
				context.InstructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Nop);
				context.InstructionWriter.DetachInstructionSequence();
			}
		}

		private void WeaveSuccess(WeavingContext context, InstructionBlock block) {
			LogLevel exitLevel = this.attribute.ExitLevel;
			if (exitLevel != LogLevel.None) {
				string message = GetMessage(context.Method, this.attribute.ExitText);

				LogLevelSupportItem supportItem = this.parent.GetSupportItem(this.attribute.ExitLevel);
				TypeDefDeclaration wovenType = context.Method.DeclaringType;
				PerTypeLoggingData perTypeLoggingData = this.parent.GetPerTypeLoggingData(wovenType);

				InstructionSequence logSuccessSequence = context.Method.MethodBody.CreateInstructionSequence();
				InstructionSequence afterLoggingSequence = context.Method.MethodBody.CreateInstructionSequence();

				block.AddInstructionSequence(logSuccessSequence, NodePosition.Before, null);
				block.AddInstructionSequence(afterLoggingSequence, NodePosition.After, logSuccessSequence);

				context.InstructionWriter.AttachInstructionSequence(logSuccessSequence);
				context.InstructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);

				// Check if the logger has debug output enabled.
				context.InstructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, perTypeLoggingData.IsLoggingEnabledField[exitLevel]);
				// Stack: isDebugEnabled.
				// Compare the isDebugEnabled to 0.
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Ldc_I4_0);
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Ceq);
				// Stack: isDebugEnabled==0.
				// If isDebugEnabled==0 then skip logging.
				context.InstructionWriter.EmitBranchingInstruction(OpCodeNumber.Brtrue_S, afterLoggingSequence);
				// Stack: .
				// Call the logging method.
				context.InstructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, perTypeLoggingData.Log);
				context.InstructionWriter.EmitInstructionString(OpCodeNumber.Ldstr, message);
				context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Callvirt, supportItem.LogStringMethod);
				// Stack: .

				// Commit changes and detach the instruction sequence.
				context.InstructionWriter.DetachInstructionSequence();

				context.InstructionWriter.AttachInstructionSequence(afterLoggingSequence);
				context.InstructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Nop);
				context.InstructionWriter.DetachInstructionSequence();
			}
		}

		private void WeaveException(WeavingContext context, InstructionBlock block) {
			LogLevel exceptionLevel = this.attribute.ExceptionLevel;
			if (this.attribute.ExceptionLevel != LogLevel.None) {
				string message = GetMessage(context.Method, this.attribute.ExceptionText);

				LogLevelSupportItem supportItem = this.parent.GetSupportItem(this.attribute.ExceptionLevel);
				TypeDefDeclaration wovenType = context.Method.DeclaringType;
				PerTypeLoggingData perTypeLoggingData = this.parent.GetPerTypeLoggingData(wovenType);

				LocalVariableSymbol localVariable = block.DefineLocalVariable(context.Method.Module.FindType(typeof(Exception), BindingOptions.Default), "~ex~{0}");

				InstructionSequence logExceptionSequence = context.Method.MethodBody.CreateInstructionSequence();
				InstructionSequence afterLoggingSequence = context.Method.MethodBody.CreateInstructionSequence();

				block.AddInstructionSequence(logExceptionSequence, NodePosition.Before, null);
				block.AddInstructionSequence(afterLoggingSequence, NodePosition.After, logExceptionSequence);

				context.InstructionWriter.AttachInstructionSequence(logExceptionSequence);
				context.InstructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
				// Stack: ex.
				// Pop the exception from the stack and store it in the variable.
				context.InstructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Stloc_S, localVariable);
				// Stack: .
				// Call log.get_IsDebugEnabled.
				context.InstructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, perTypeLoggingData.IsLoggingEnabledField[exceptionLevel]);
				// Stack: isDebugEnabled.
				// Push 0 on the stack and check if isDebugEnabled is equal to 0.
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Ldc_I4_0);
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Ceq);
				// Stack: isDebugEnabled==0.
				// If isDebugEnabled==0 then skip logging.
				context.InstructionWriter.EmitBranchingInstruction(OpCodeNumber.Brtrue_S, afterLoggingSequence);
				// Stack: .
				// Call log.Debug(message, ex).
				context.InstructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, perTypeLoggingData.Log);
				context.InstructionWriter.EmitInstructionString(OpCodeNumber.Ldstr, message);
				context.InstructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc_S, localVariable);
				context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Callvirt, supportItem.LogStringExceptionMethod);
				// Stack: .
				context.InstructionWriter.DetachInstructionSequence();

				context.InstructionWriter.AttachInstructionSequence(afterLoggingSequence);
				// Stack: .
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Rethrow);
				context.InstructionWriter.DetachInstructionSequence();
			} else {
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
            // TODO: Use another property LogAttribute.AspectPriority
            // AttributePriority makes sense only during the multicasting process.
			get { return this.attribute.AttributePriority; }
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