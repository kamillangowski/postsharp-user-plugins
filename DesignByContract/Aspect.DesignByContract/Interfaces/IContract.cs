/*

Copyright (c) 2008, Patrick Jahnke

All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of the Patrick Jahnke nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

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
using System.Text;

namespace Aspect.DesignByContract.Interface
{
    public interface IContract
    {
        /// <summary>
        /// Prüft den Kontrakt der erzeugt wurde und über contractKey aufgerufen werden soll.
        /// </summary>
        /// <param name="contractKey">Schlüssel mit dem identifiziert wird, welcher Kontrakt geprüft werden soll.</param>
        /// <param name="contractArguments">Übergabeargumente</param>
        /// <param name="instance">Konkretes Objekt in dem der Kontrakt geprüft werden soll.</param>
        /// <param name="methodResult">Das Ergebnis welches zurückgeliefert wird bei Eigenschaften 
        /// und Methoden die void zurückgeben ist der Parameter immer null.</param>
		/// <param name="oldValues">Eine Liste mit allen Zwischengespeicherten Werten</param>
		/// <param name="genericTypes">Eine Liste mit den Generischen Typen des Objekts instance</param>
		void CheckContract(string contractKey, object[] contractArguments, object instance, object methodResult, Type[] genericTypes, object[] oldValues);

		/// <summary>
		/// Gibt die Werte zurück die als oldParameter deklariert wurden.
		/// </summary>
		/// <param name="contractKey">Schlüssel mit dem identifiziert wird, welcher Kontrakt geprüft werden soll.</param>
		/// <param name="contractArguments">Übergabeargumente</param>
		/// <param name="instance">Konkretes Objekt in dem der Kontrakt geprüft werden soll.</param>
		/// <param name="genericTypes">Eine Liste mit den Generischen Typen des Objekts instance</param>
		/// <returns>Liste mit allen Werten die zwischengespeichert werden</returns>		
		object[] GetOldValues(string contractKey, object[] contractArguments, object instance, Type[] genericTypes);

	}
}
