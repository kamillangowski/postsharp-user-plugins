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

using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeWeaver;

namespace Log4PostSharp.Weaver
{
  /// <summary>
  /// Represents the token that expands to the return value of the method.
  /// </summary>
  public class ReturnValueToken : IMessageToken
  {
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
    public ReturnValueToken(ParameterDeclaration returnParameter)
    {
      if (returnParameter == null)
      {
        throw new ArgumentNullException("returnParameter");
      }

      this.returnParameter = returnParameter;
    }

    #endregion

    #region IMessageToken Members

    public bool IsStatic
    {
      get { return false; }
    }

    public string Text
    {
      get { throw new NotSupportedException(); }
    }

    public void Emit(WeavingContext context)
    {
      if (context == null)
      {
        throw new ArgumentNullException("context");
      }

      // Ensure that the there is any return value.
      if (context.ReturnValueVariable != null)
      {
        context.InstructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, context.ReturnValueVariable);
        context.InstructionWriter.EmitConvertToObject(this.returnParameter.ParameterType);
      }
      else
      {
        context.InstructionWriter.EmitInstruction(OpCodeNumber.Ldnull);
      }
    }

    #endregion
  }
}