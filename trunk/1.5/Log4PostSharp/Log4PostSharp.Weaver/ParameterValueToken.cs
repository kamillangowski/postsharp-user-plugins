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

using PostSharp.CodeModel;
using PostSharp.CodeWeaver;

namespace Log4PostSharp.Weaver
{
  /// <summary>
  /// Represents the token which expands to the value of the method parameter.
  /// </summary>
  /// <remarks>
  /// <para>Since values of method parameters are known only at run-time, this is dynamic token.</para>
  /// </remarks>
  public class ParameterValueToken : IMessageToken
  {
    #region Private Fields

    /// <summary>
    /// Parameter whose value this token expands to.
    /// </summary>
    private readonly ParameterDeclaration parameter;

    #endregion

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterValueToken"/> class
    /// with the specified parameter.
    /// </summary>
    /// <param name="parameter">Parameter whose value the token expands to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is <see langword="null"/>.</exception>
    public ParameterValueToken(ParameterDeclaration parameter)
    {
      if (parameter == null)
      {
        throw new ArgumentNullException("parameter");
      }

      this.parameter = parameter;
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

      context.InstructionWriter.EmitInstructionParameter(OpCodeNumber.Ldarg, this.parameter);
      context.WeavingHelper.ToObject(this.parameter.ParameterType, context.InstructionWriter);
    }

    #endregion
  }
}