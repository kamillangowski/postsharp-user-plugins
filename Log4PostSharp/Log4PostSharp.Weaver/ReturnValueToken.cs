using System;

using PostSharp.CodeModel;
using PostSharp.CodeWeaver;

namespace Log4PostSharp.Weaver {
	/// <summary>
	/// Represents the token that expands to the return value of the method.
	/// </summary>
	public class ReturnValueToken : IMessageToken {
		#region Private Fields

		/// <summary>
		/// Metadata for the method return parameter.
		/// </summary>
		private readonly ParameterDeclaration returnParameter;

		#endregion

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ReturnValueToken"/> class
		/// with the specified return parameter metadata.
		/// </summary>
		/// <param name="returnParameter">Return parameter metadata.</param>
		/// <exception cref="ArgumentNullException"><paramref name="returnParameter"/> is <see langword="null"/>.</exception>
		public ReturnValueToken(ParameterDeclaration returnParameter) {
			if (returnParameter == null) {
				throw new ArgumentNullException("returnParameter");
			}

			this.returnParameter = returnParameter;
		}

		#endregion

		#region IMessageToken Members

		public bool IsStatic {
			get { return false; }
		}

		public string Text {
			get { throw new NotSupportedException(); }
		}

		public void Emit(WeavingContext context) {
			if (context == null) {
				throw new ArgumentNullException("context");
			}

			// Ensure that the there is any return value.
			if (context.ReturnValueVariable != null) {
				context.InstructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, context.ReturnValueVariable);
				context.WeavingHelper.ToObject(this.returnParameter.ParameterType, context.InstructionWriter);
			} else {
				context.InstructionWriter.EmitInstruction(OpCodeNumber.Ldnull);
			}
		}

		#endregion
	}
}