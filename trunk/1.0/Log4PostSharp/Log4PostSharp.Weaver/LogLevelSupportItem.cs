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

namespace Log4PostSharp.Weaver {
	/// <summary>
	/// Contains information needed to log messages using certain log level.
	/// </summary>
	internal class LogLevelSupportItem {
		/// <summary>
		/// log4net.ILog.IsXXXEnabled property getter.
		/// </summary>
		private readonly IMethod isLoggingEnabledGetter;

		/// <summary>
		/// log4net.ILog.XXX(string) method.
		/// </summary>
		private readonly IMethod logStringMethod;

		/// <summary>
		/// log4net.ILog.XXX(string, Exception) method.
		/// </summary>
		private readonly IMethod logStringExceptionMethod;

		/// <summary>
		/// log4net.ILog.XXX(CultureInfo, string, object[]) method.
		/// </summary>
		private readonly IMethod logCultureStringArgsMethod;

		/// <summary>
		/// Gets the log4net.ILog.IsXXXEnabled property getter.
		/// </summary>
		public IMethod IsLoggingEnabledGetter {
			get { return this.isLoggingEnabledGetter; }
		}

		/// <summary>
		/// Gets the log4net.ILog.XXX(string) method.
		/// </summary>
		public IMethod LogStringMethod {
			get { return this.logStringMethod; }
		}

		/// <summary>
		/// Gets the log4net.ILog.XXX(string, Exception) method.
		/// </summary>
		public IMethod LogStringExceptionMethod {
			get { return this.logStringExceptionMethod; }
		}

		/// <summary>
		/// Gets the log4net.ILog.XXX(CultureInfo, string, object[]) method.
		/// </summary>
		public IMethod LogCultureStringArgsMethod {
			get { return this.logCultureStringArgsMethod; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LogLevelSupportItem"/> class with the specified methods.
		/// </summary>
		/// <param name="isLoggingEnabledGetter">Getter of log4net.ILog.IsXXXEnabled property.</param>
		/// <param name="logStringMethod">log4net.ILog.XXX(string) method.</param>
		/// <param name="logStringExceptionMethod">log4net.ILog.XXX(string, Exception) method.</param>
		/// <param name="logCultureStringArgsMethod">log4net.ILog.XXX(CultureInfo, string, object[]) method.</param>
		/// <exception cref="ArgumentNullException"><paramref name="isLoggingEnabledGetter"/>, <paramref name="logStringMethod"/>, <paramref name="logStringExceptionMethod"/> or <paramref name="logCultureStringArgsMethod"/> is <see langword="null"/>.</exception>
		public LogLevelSupportItem(IMethod isLoggingEnabledGetter, IMethod logStringMethod, IMethod logStringExceptionMethod, IMethod logCultureStringArgsMethod) {
			if (isLoggingEnabledGetter == null) {
				throw new ArgumentNullException("isLoggingEnabledGetter");
			}
			if (logStringMethod == null) {
				throw new ArgumentNullException("logStringMethod");
			}
			if (logStringExceptionMethod == null) {
				throw new ArgumentNullException("logStringExceptionMethod");
			}
			if (logCultureStringArgsMethod == null) {
				throw new ArgumentNullException("logCultureStringArgsMethod");
			}

			this.isLoggingEnabledGetter = isLoggingEnabledGetter;
			this.logCultureStringArgsMethod = logCultureStringArgsMethod;
			this.logStringMethod = logStringMethod;
			this.logStringExceptionMethod = logStringExceptionMethod;
		}
	}
}