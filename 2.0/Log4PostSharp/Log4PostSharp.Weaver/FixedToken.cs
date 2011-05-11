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
  /// Represents the token that contains fixed string which is known at weave-time.
  /// </summary>
  public class FixedToken : IMessageToken
  {
    #region Private Fields

    /// <summary>
    /// Fixed contents of this token.
    /// </summary>
    private readonly string text;

    #endregion

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedToken"/> class
    /// with the specified contents.
    /// </summary>
    /// <param name="text">Contents for the token.</param>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    public FixedToken(string text)
    {
      if (text == null)
      {
        throw new ArgumentNullException("text");
      }

      this.text = text;
    }

    #endregion

    #region IMessageToken Members

    public bool IsStatic
    {
      get { return true; }
    }

    public string Text
    {
      get { return this.text; }
    }

    public void Emit(WeavingContext context)
    {
      if (context == null)
      {
        throw new ArgumentNullException("context");
      }

      context.InstructionWriter.EmitInstructionString(OpCodeNumber.Ldstr, this.Text);
    }

    #endregion
  }
}