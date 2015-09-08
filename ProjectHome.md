Plug-ins and Aspect Libraries contributed by the [PostSharp](http://www.postsharp.org/) community.

This project currently contains the following components:

  * Log4PostSharp: trace your programs using a single custom attribute. Log4PostSharp emits optimal instructions for you. Yes, just like hand-tuned code!

  * PostSharp4Unity: Mark classes as configurable and start enjoying [Unity](http://www.codeplex.com/unity) without the factory method! Old plain constructor still work.

  * PostSharpAspNet: Enables to use PostSharp in ASP.NET projects even with JIT compilation.

  * DesignByContract: transparently adds preconditions, postconditions and invariant to methods and classes. Just beginning, a lot of work to do.

  * [Awareness](Awareness.md): makes PostSharp Laos aware of serialization (BinaryFormatter and WCF).

  * [GSharp](GSharp.md): One of the classic uses of AOP is to automate logging.  But an unfortunate side-effect of logging everywhere is massive log files that are hard to read, hard to dig around in, and sometimes hard to just open without running out of memory. GSharp aspects let you trace your programs as well as log exceptions and profile performance.  You can then analyze and graph your data with [Gibraltar](http://www.gibraltarsoftware.com/See/PostSharp.aspx).