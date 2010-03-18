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

using Log4PostSharp;

namespace Log4PostSharpDemo
{
    public class Program
    {
        [Log(EntryLevel = LogLevel.Debug, EntryText = "Adding {@i1} to {@i2}.", ExitLevel = LogLevel.Debug, ExitText = "Result of addition is {returnvalue}.")]
        private static int Add(int i1, int i2)
        {
            return i1 + i2;
        }

        [Log(EntryLevel = LogLevel.Debug, ExitLevel = LogLevel.Debug, ExceptionLevel = LogLevel.Fatal)]
        private static int ReadNumber()
        {
            // Display prompt.
            Console.Write("Please enter a number: ");

            // Read the line from the console.
            string line = Console.ReadLine();

            // Convert the data into the integer.
            return int.Parse(line, CultureInfo.CurrentCulture);
        }

        public static void Main(string[] args)
        {
            // Get operands.
            int i1 = ReadNumber();
            int i2 = ReadNumber();

            // Calculate the sum.
            int sum = Add(i1, i2);

            // Print the result.
            Console.WriteLine("Result is {0}.", sum);
        }
    }
}
