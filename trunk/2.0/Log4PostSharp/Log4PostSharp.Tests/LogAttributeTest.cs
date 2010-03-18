using System;
using System.Diagnostics;
using log4net;
using log4net.Appender;
using log4net.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Log4PostSharp.Test
{
  ///<summary>
  ///This is a test class for <see cref="LogAttribute"/>.
  ///</summary>
  [TestClass()]
  public class LogAttributeTest
  {
    #region Helper Classes

    private static class AppenderHolder
    {
      public static MemoryAppender Appender
      {
        get
        {
          IAppender[] appenders = LogManager.GetRepository().GetAppenders();
          foreach (IAppender appender in appenders)
          {
            if (string.Equals(appender.Name, "TestingAppender", StringComparison.OrdinalIgnoreCase))
            {
              return (MemoryAppender)appender;
            }
          }

          Debug.Fail("Could not find dedicated MemoryAppender for testing.");
          throw new InvalidOperationException();
        }
      }
    }

    #endregion

    #region Helper Methods

    private static void ValidateEvents(params string[] expected)
    {
      LoggingEvent[] events = AppenderHolder.Appender.GetEvents();
      AppenderHolder.Appender.Clear();

      Assert.AreEqual(expected.Length, events.Length);
      for (int i = 0; i < events.Length; ++i)
      {
        Assert.AreEqual(expected[i], events[i].RenderedMessage);
      }
    }

    [Log(EntryLevel = LogLevel.Debug, EntryText = "{method}({paramvalues})", ExitLevel = LogLevel.Debug, ExitText = "{method}({paramvalues}) = {returnvalue}", IncludeParamName = true)]
    private bool WithParamNames(int i, string s, ref double d, out object obj)
    {
      d = 0.1;
      obj = typeof(int);
      return true;
    }

    [Log(EntryLevel = LogLevel.Debug, EntryText = "{method}({inparamvalues})", ExitLevel = LogLevel.Debug, ExitText = "{method}({inparamvalues}) = {returnvalue}({outparamvalues})", IncludeParamName = true)]
    private bool WithParamNamesInOut(int i, string s, ref double d, out object obj)
    {
      d = 0.1;
      obj = typeof(int);
      return true;
    }

    [Log(EntryLevel = LogLevel.Debug, EntryText = "{method}({paramvalues})", ExitLevel = LogLevel.Debug, ExitText = "{method}({paramvalues}) = {returnvalue}", IncludeParamName = false)]
    private bool NoParamNames(int i, string s, ref double d, out object obj)
    {
      d = 0.1;
      obj = typeof(int);
      return true;
    }

    [Log(EntryLevel = LogLevel.None, ExitLevel = LogLevel.Debug, ExitText = "{signature}")]
    private bool Signature(int i, string s, ref double d, out object obj)
    {
      d = 0.1;
      obj = typeof(int);
      return true;
    }

    #endregion

    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get { return testContextInstance; }
      set { testContextInstance = value; }
    }

    [TestInitialize]
    public void Init()
    {
        AppenderHolder.Appender.Clear();
    }

    #region Test Methods

    [TestMethod]
    public void WithParamNames()
    {
      double d = 1.3;
      object o;
      WithParamNames(1, "Hello", ref d, out o);
      ValidateEvents("WithParamNames(i: 1, s: \"Hello\", d: 1.3, obj: )",
        "WithParamNames(i: 1, s: \"Hello\", d: 0.1, obj: System.Int32) = True");
    }

    [TestMethod]
    public void WithParamNamesInOut()
    {
      double d = 1.3;
      object o;
      WithParamNamesInOut(1, "Hello", ref d, out o);
      ValidateEvents("WithParamNamesInOut(i: 1, s: \"Hello\", d: 1.3)",
        "WithParamNamesInOut(i: 1, s: \"Hello\", d: 0.1) = True(d: 0.1, obj: System.Int32)");
    }

    [TestMethod]
    public void NoParamNames()
    {
      double d = 1.3;
      object o;
      NoParamNames(1, "Hello", ref d, out o);
      ValidateEvents("NoParamNames(1, \"Hello\", 1.3, )",
        "NoParamNames(1, \"Hello\", 0.1, System.Int32) = True");
    }

    [TestMethod]
    public void Signature()
    {
      double d = 1.3;
      object o;
      Signature(1, "Hello", ref d, out o);
      ValidateEvents("Boolean Signature(Int32, System.String, Double ByRef, System.Object ByRef)");
    }

    #endregion
  }
}
