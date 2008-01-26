using System;

namespace Log4PostSharp.Test {
	public class Program {
		public static void MethodWithNoReturnValue() {
		}

		public static void MethodWithNoReturnValue(Guid someId) {
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
			MethodWithNoReturnValue();
			MethodWithNoReturnValue(Guid.NewGuid());
			MethodWithGuidReturnValue();
			MethodWithIntReturnValue();
		}

		public class Subprogram {
			[Log]
			public Subprogram() {
			}
		}
	}
}
