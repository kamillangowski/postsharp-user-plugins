using System;
using System.Collections.Generic;
using System.Globalization;

namespace Log4PostSharp.TestApp
{
    public class Program
    {
        public static void MethodWithNoReturnValue()
        {
        }

        [Log(EntryLevel = LogLevel.Fatal, EntryText = "Calling {@someId}.")]
        [Log(ExitLevel = LogLevel.Fatal, ExitText = "Return value is: '{returnvalue}'.")]
        public static void MethodWithNoReturnValue(Guid someId)
        {
        }

        [Log(EntryLevel = LogLevel.Debug, EntryText = "Test for '{@i1}', '{@i2}', '{@r1}', '{@r2}'.")]
        [Log(ExitLevel = LogLevel.Fatal, ExitText = "Was called with params: {paramvalues}.")]
        [Log(ExitLevel = LogLevel.Fatal, ExitText = "Return value is: '{returnvalue}'.")]
        public static int MultipleArgsMethod(int i1, string i2, ref int r1, ref object r2, out Guid o1, out string o2)
        {
            o1 = Guid.NewGuid();
            o2 = "X";
            return i1 + 50;
        }

        [Log(LogLevel.Info, "Test message.")]
        public static Guid MethodWithGuidReturnValue()
        {
            return Guid.NewGuid(); 
        }

        [Log(EntryLevel = LogLevel.Info, EntryText = "Doing stuff.", ExitLevel = LogLevel.None, ExceptionLevel = LogLevel.None)]
        public static int MethodWithIntReturnValue()
        {
            return int.MinValue;
        }

        public static void Main(string[] args)
        {
            string.Format(CultureInfo.InvariantCulture, "{0}{1}", args, 5);

            MethodWithNoReturnValue();
            MethodWithNoReturnValue(Guid.NewGuid());
            int r1 = 10;
            object r2 = "X";
            Guid o1;
            string o2;
            MultipleArgsMethod(1, "A", ref r1, ref r2, out o1, out o2);

            MethodWithGuidReturnValue();
            MethodWithIntReturnValue();

            new AnotherSubprogram<int>().Act<string>();
        }

        public class SubclassWithCompilerGeneratedCode
        {
            public IEnumerable<string> MethodThatAddsCompilerGeneratedCode()
            {
                yield return "A";
                yield return "B";
            }
        }

        public class Subprogram
        {
            [Log]
            public Subprogram()
            {
            }
        }

        public class AnotherSubprogram<T>
        {
            static readonly object x = new object();

            public T DoStuff(T t)
            {
                return default(T);
            }

            public U Act<U>()
            {
                x.ToString();

                return default(U);
            }

            public W Act<U, V, W>(U u, V v)
            {
                return default(W);
            }
        }

        public interface ITest
        {
            void DoTest();
        }
    }
}
