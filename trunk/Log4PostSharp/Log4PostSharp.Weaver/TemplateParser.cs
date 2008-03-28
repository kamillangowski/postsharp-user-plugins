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

namespace Log4PostSharp.Weaver {
	/// <summary>
	/// Provides methods for parsing log message templates.
	/// </summary>
	public static class TemplateParser {
		#region Private Fields

		/// <summary>
		/// Character that indicates beginning of a placeholder.
		/// </summary>
		private static readonly char beginOfSequenceMarker = '{';

		/// <summary>
		/// Character that indicates beginning of a placeholder.
		/// </summary>
		private static readonly char endOfSequenceMarker = '}';

		/// <summary>
		/// Long version of placeholder that indicates method signature token.
		/// </summary>
		private static readonly string signaturePlaceholder1 = "signature";

		/// <summary>
		/// Short version of placeholder that indicates method signature token.
		/// </summary>
		private static readonly string signaturePlaceholder2 = "sig";

		/// <summary>
		/// Placeholder that indicates comma-separated list of values of all method parameters.
		/// </summary>
		private static readonly string shortParameterList = "paramlist";

		/// <summary>
		/// Prefix that indicates method parameter token.
		/// </summary>
		private static readonly string parameterPrefix = "@";

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets text containing the signature of the specified method.
		/// </summary>
		/// <param name="method">Method.</param>
		/// <returns>Signature of the specified method.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		private static string GetMethodSignature(IMethod method) {
			if (method == null) {
				throw new ArgumentNullException("method");
			}

			StringBuilder builder = new StringBuilder();
			method.WriteReflectionMethodName(builder, ReflectionNameOptions.MethodParameterEncoding);
			return builder.ToString();
		}

		/// <summary>
		/// Creates token for the specified placeholder.
		/// </summary>
		/// <param name="placeholder">Placeholder to create token for.</param>
		/// <param name="target">List to appent the token to.</param>
		/// <param name="wovenMethod">Method being woven.</param>
		/// <exception cref="ArgumentNullException"><paramref name="placeholder"/>, <paramref name="target"/> or <paramref name="wovenMethod"/> is <see langword="null"/>.</exception>
		/// <exception cref="FormatException"><paramref name="placeholder"/> is invalid or unrecognized placeholder.</exception>
		private static void ProcessPlaceholder(string placeholder, ICollection<IMessageToken> target, MethodDefDeclaration wovenMethod) {
			if (placeholder == null) {
				throw new ArgumentNullException("placeholder");
			}
			if (target == null) {
				throw new ArgumentNullException("target");
			}
			if (wovenMethod == null) {
				throw new ArgumentNullException("wovenMethod");
			}

			if (string.Equals(placeholder, signaturePlaceholder2, StringComparison.InvariantCulture)
			    || string.Equals(placeholder, signaturePlaceholder1, StringComparison.InvariantCulture)) {
				target.Add(new FixedToken(GetMethodSignature(wovenMethod)));
			} else if (placeholder.StartsWith(parameterPrefix, StringComparison.InvariantCulture)) {
				// Extract the name of the parameter.
				string parameterName = placeholder.Substring(parameterPrefix.Length);

				ParameterDeclaration referredParameter = null;
				foreach (ParameterDeclaration parameter in wovenMethod.Parameters) {
					if (string.Equals(parameter.Name, parameterName, StringComparison.InvariantCulture)) {
						referredParameter = parameter;
						break;
					}
				}
				if (referredParameter == null) {
					throw new FormatException(string.Format(CultureInfo.CurrentCulture, "Invalid parameter name: {0}.", parameterName));
				}

				target.Add(new ParameterValueToken(referredParameter));
			} else if (string.Equals(placeholder, shortParameterList, StringComparison.InvariantCulture)) {
				// Check if the method has any parameters.
				if (wovenMethod.Parameters.Count > 0) {
					// Add opening quote for the first parameter.
					target.Add(new FixedToken(@""""));

					bool isFirstParameter = true;
					foreach (ParameterDeclaration parameter in wovenMethod.Parameters) {
						// Do not prepend anything before the first parameter.
						if (! isFirstParameter) {
							// Add closing quote for the previous parameter, then comma, then opening quote for the current parameter.
							target.Add(new FixedToken(@""", """));
						}

						// Append parameter value.
						target.Add(new ParameterValueToken(parameter));

						// Next parameter is not the first one.
						isFirstParameter = false;
					}

					// Add closing quote for the last parameter.
					target.Add(new FixedToken(@""""));
				}
			} else {
				throw new FormatException(string.Format(CultureInfo.CurrentCulture, "Unknown placeholder in template: {0}.", placeholder));
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Splits the template into the list of tokens.
		/// </summary>
		/// <param name="template">Template to find tokens in.</param>
		/// <param name="wovenMethod">Method being woven.</param>
		/// <returns>List of tokens in the specified template.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="template"/> or <paramref name="wovenMethod"/> is <see langword="null"/>.</exception>
		/// <exception cref="FormatException"><paramref name="template"/> is invalid and cannot be parsed.</exception>
		public static List<IMessageToken> Tokenize(string template, MethodDefDeclaration wovenMethod) {
			if (template == null) {
				throw new ArgumentNullException("template");
			}
			if (wovenMethod == null) {
				throw new ArgumentNullException("wovenMethod");
			}

			// List to put all recognized message parts in.
			List<IMessageToken> ret = new List<IMessageToken>();

			// Algorithm parses the messages by fragments. Boundaries of fragments are special characters,
			// like sequence indicators.

			// Position of the first character of the fragment that is currently processed.
			int fragmentBeginningIndex = 0;
			while (fragmentBeginningIndex < template.Length) {
				// Position of the first character of the next fragment.
				int nextFragmentBeginningIndex;

				// Search for the beginning-of-special-sequence indicator.
				int sequenceBeginningIndex = template.IndexOf(beginOfSequenceMarker, fragmentBeginningIndex);
				if (sequenceBeginningIndex != -1) {
					// Special sequence indicator has been found.

					// Append to the buffer the text preceding the marker.
					ret.Add(new FixedToken(template.Substring(fragmentBeginningIndex, sequenceBeginningIndex - fragmentBeginningIndex)));

					// Ensure that the special character is not the last one in the template.
					if (sequenceBeginningIndex + 1 < template.Length) {
						// Check if the next character in the template is again the sequence marker.
						char nextTemplateChar = template[sequenceBeginningIndex + 1];
						if (nextTemplateChar != beginOfSequenceMarker) {
							// Find the end-of-sequence marker.
							int sequenceEndingIndex = template.IndexOf(endOfSequenceMarker, sequenceBeginningIndex);
							if (sequenceEndingIndex != -1) {
								// Obtain the placeholder.
								string placeholder = template.Substring(sequenceBeginningIndex + 1, sequenceEndingIndex - sequenceBeginningIndex - 1);
								ProcessPlaceholder(placeholder, ret, wovenMethod);

								// Next fragment starts right after the sequence.
								nextFragmentBeginningIndex = sequenceEndingIndex + 1;
							} else {
								// Sequence is not closed.
								throw new FormatException(string.Format(CultureInfo.CurrentCulture, "Unfinished placeholder in template: {0}.", template));
							}
						} else {
							// Repeated begin-of-sequence marker, indicates the marker should be used literally.
							ret.Add(new FixedToken(beginOfSequenceMarker.ToString(CultureInfo.InvariantCulture)));
							nextFragmentBeginningIndex = sequenceBeginningIndex + 2;
						}
					} else {
						// Begin-of-sequence marker is the last char in the template.
						throw new FormatException(string.Format(CultureInfo.CurrentCulture, "Invalid placeholder in template: {0}.", template));
					}
				} else {
					// No special sequence indicator has been found.

					// Copy the remaining part of the template as literal message text.
					ret.Add(new FixedToken(template.Substring(fragmentBeginningIndex, template.Length - fragmentBeginningIndex)));
					// Set the index to the position which causes the parsing loop to exit.
					nextFragmentBeginningIndex = template.Length;
				}

				fragmentBeginningIndex = nextFragmentBeginningIndex;
			}

			return ret;
		}

		#endregion
	}
}