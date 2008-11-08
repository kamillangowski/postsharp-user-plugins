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

using PostSharp.CodeWeaver;

namespace Log4PostSharp.Weaver {
	/// <summary>
	/// Represents a part of log message.
	/// </summary>
	public interface IMessageToken {
		/// <summary>
		/// Gets a value indicating whether context of the token can be determined at weave-time.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the contents of this token can be determined at weave-time or
		/// <see langword="false"/> otherwise.
		/// </value>
		bool IsStatic { get; }

		/// <summary>
		/// Gets the contents of the token.
		/// </summary>
		/// <value>
		/// Contents of the token.
		/// </value>
		/// <exception cref="NotSupportedException">Contents of the token can be determined only at run-time (<see cref="IsStatic"/> is <see langword="false"/>).</exception>
		string Text { get; }

		/// <summary>
		/// Emits sequence of instructions that pushes the contents of the token onto the evaluation stack.
		/// </summary>
		/// <param name="context">Weaving context.</param>
		/// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>Code emitted by this method does not make any assumptions on the state of the evaluation
		/// stack, but it modifies the stack by pushing the reference to the object that represents the
		/// token contents. The object can be included in the array of arguments that is later passed
		/// to the <see cref="string.Format(IFormatProvider,string,object[])"/> (or compatible) method.</para>
		/// </remarks>
		void Emit(WeavingContext context);
	}
}