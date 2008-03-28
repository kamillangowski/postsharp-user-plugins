using System;
using System.Globalization;

namespace Log4PostSharp.Test {
	public class Program {
		public static void MethodWithNoReturnValue() {
		}

		[Log(EntryLevel = LogLevel.Fatal, EntryText = "Calling {@someId}.")]
		public static void MethodWithNoReturnValue(Guid someId) {
		}

		[Log(EntryLevel = LogLevel.Debug, EntryText = "Test for '{@a1}', '{@a2}', '{@a3}'.")]
		[Log(ExitLevel = LogLevel.Fatal, ExitText = "Was called with params: {paramvalues}.")]
		public static int MultipleArgsMethod(int a1, string a2, object a3) {
			return a1 + 50;
		}

		[Log(LogLevel.Info, "Test message.")]
		public static Guid MethodWithGuidReturnValue() {
			return Guid.NewGuid();
		}

		[Log(EntryLevel = LogLevel.Info, EntryText = "Doing stuff.", ExitLevel = LogLevel.None, ExceptionLevel = LogLevel.None)]
		public static int MethodWithIntReturnValue() {
			return int.MinValue;
		}

		public static void Main(string[] args) {
			string.Format(CultureInfo.InvariantCulture, "{0}{1}", args, 5);

			MethodWithNoReturnValue();
			MethodWithNoReturnValue(Guid.NewGuid());
			MultipleArgsMethod(1, "A", null);
			MethodWithGuidReturnValue();
			MethodWithIntReturnValue();

			new AnotherSubprogram<int>().Act<string>();
		}

		public class Subprogram {
			[Log]
			public Subprogram() {
			}
		}

		public class AnotherSubprogram<T> {
			static readonly object x = new object();

			public T DoStuff(T t) {
				return default(T);
			}

			public U Act<U>() {
				x.ToString();

				return default(U);
			}

			public W Act<U, V, W>(U u, V v) {
				return default(W);
			}
		}

		public interface ITest {
			void DoTest();
		}
	}
}
