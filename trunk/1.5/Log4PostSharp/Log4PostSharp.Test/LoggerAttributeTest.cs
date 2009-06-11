using System;
using System.Diagnostics;
using log4net;
using log4net.Appender;
using log4net.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Log4PostSharp.Test
{
  /// <summary>
  ///This is a test class for LoggerAttributeTest and is intended
  ///to contain all LoggerAttributeTest Unit Tests
  ///</summary>
  ///<remarks>
  /// Each test method will run in a separate <see cref="AppDomain"/>|. This fact will ensure
  /// expected regsitration of logger names per types since the LoggerHelper is a static class.
  ///</remarks>
  [TestClass()]
  public class LoggerAttributeTest
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

    [Log]
    [Logger("SimpleClass")]
    private class SimpleClassA { }

    [Log]
    [Logger(LoggerNamePolicy.AssemblyQualifiedName)]
    private class SimpleClassB { }

    [Log]
    private class SimpleClassC { }

    [Log]
    [Logger("GenericArg")]
    private class GenericArgA { }

    [Log]
    [Logger("GenericArg")]
    private class GenericArgB { }

    [Log]
    [Logger("GenericClass", LoggerNamePolicy.TypeName)]
    private class GenericClassA<T> { }

    [Log]
    [Logger("SimpleClass")] //Collides with SimpleClassA
    private class CollidingClassA { }

    [Logger("NoLogAttribute")]
    private class NoLogAttributeClass { }

    [Logger(LoggerNamePolicy.TypeName)]
    private static class StaticClassA
    {
      [Log]
      public static void DoIt() { }
    }

    #endregion

    #region Helper Methods

    private static void ValidateEvents(string[] loggerNames)
    {
      LoggingEvent[] events = AppenderHolder.Appender.GetEvents();
      Assert.AreEqual(loggerNames.Length, events.Length);

      for (int i = 0; i < loggerNames.Length; i++)
      {
        Assert.AreEqual(loggerNames[i], events[i].LoggerName);
      }

    }

    private static void RunIsolated(CrossAppDomainDelegate del)
    {
      var domain = AppDomain.CreateDomain(string.Empty, null, AppDomain.CurrentDomain.SetupInformation);
      domain.DoCallBack(del);
      AppDomain.Unload(domain);
    }

    #endregion

    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    #region Test Initialization/Cleanup

    [TestInitialize()]
    public void TestInitialize()
    {
    }

    #endregion

    #region Test Methods

    [TestMethod]
    public void TestSimpleClassWithExplicitName()
    {
      RunIsolated(() =>
      {
        new SimpleClassA();
        ValidateEvents(new string[] { "SimpleClass" });
      });
    }

    [TestMethod]
    public void TestSimpleClassAsseblyQualified()
    {
      RunIsolated(() =>
      {
        new SimpleClassB();
        ValidateEvents(new string[] { typeof(SimpleClassB).AssemblyQualifiedName });
      });
    }

    [TestMethod]
    public void TestSimpleClassWithDefault()
    {
      RunIsolated(() =>
      {
        new SimpleClassC();
        string expectedName = string.Empty;
        switch (LoggerHelper.DefaultLoggerNamePolicy)
        {
        case LoggerNamePolicy.AssemblyQualifiedName:
          expectedName = typeof(SimpleClassC).AssemblyQualifiedName;
          break;
        case LoggerNamePolicy.FullTypeName:
          expectedName = typeof(SimpleClassC).FullName;
          break;
        case LoggerNamePolicy.TypeName:
          expectedName = typeof(SimpleClassC).Name;
          break;
        default:
          Assert.Fail("Unknown type of LoggerHelper.DefaultLoggerNamePolicy.");
          break;
        }

        ValidateEvents(new string[] { expectedName });
      });
    }

    [TestMethod]
    public void TestSimpleGeneric()
    {
      RunIsolated(() =>
      {
        new GenericClassA<int>();
        ValidateEvents(new string[] { "GenericClass<" + typeof(int).Name + ">" });
      });
    }

    [TestMethod]
    public void TestGenericWithRegisteredArg()
    {
      RunIsolated(() =>
      {
        new SimpleClassA();
        new GenericClassA<SimpleClassA>();
        ValidateEvents(new string[] { "SimpleClass", "GenericClass<SimpleClass>" });
      });
    }

    [TestMethod]
    public void TestSimpleClassNameCollision()
    {
      RunIsolated(() =>
      {
        LoggerHelper.ReportNameCollisions = false;

        new SimpleClassA();
        new CollidingClassA(); //Logger name is the same as of SimpleClassA

        ValidateEvents(new string[] { "SimpleClass", typeof(CollidingClassA).AssemblyQualifiedName });
      });
    }

    [TestMethod]
    public void TestSingleGenericArgRegisteredName()
    {
      RunIsolated(() =>
      {
        LoggerHelper.ReportNameCollisions = false;

        new SimpleClassA();
        new CollidingClassA();
        new GenericClassA<CollidingClassA>();

        ValidateEvents(new string[] { "SimpleClass", 
              typeof(CollidingClassA).AssemblyQualifiedName,
              "GenericClass<" + typeof(CollidingClassA).AssemblyQualifiedName + ">"});
      });
    }

    [TestMethod]
    public void TestSingleGenericArgNotRegisteredNameCollision()
    {
      RunIsolated(() =>
      {
        new SimpleClassA();
        new GenericClassA<CollidingClassA>();

        //CollidingClassA is not regsitered.
        ValidateEvents(new string[] { "SimpleClass", "GenericClass<SimpleClass>" });
      });
    }

    [TestMethod]
    public void TestCollisionReporting()
    {
      RunIsolated(() =>
      {
        LoggerHelper.ReportNameCollisions = true;
        new SimpleClassA();
        new CollidingClassA(); //Logger name is the same as of SimpleClassA

        ValidateEvents(new string[] {
          "SimpleClass", 
          "LoggerHelper", //the collistion report.
          typeof(CollidingClassA).AssemblyQualifiedName 
        });
      });
    }

    [TestMethod]
    public void TestNoLogAttribute()
    {
      RunIsolated(() =>
      {
        new NoLogAttributeClass();
        LoggerHelper.GetLogger(typeof(NoLogAttributeClass)).Info("Testing");
        ValidateEvents(new string[] { "NoLogAttribute" });
      });
    }

    [TestMethod]
    public void TestGenericArgsNameCollision()
    {
      RunIsolated(() =>
      {
        LoggerHelper.ReportNameCollisions = false;

        new GenericClassA<GenericArgA>();
        new GenericClassA<GenericArgB>();

        ValidateEvents(new string[]{
              "GenericClass<GenericArg>",
              typeof(GenericClassA<GenericArgB>).AssemblyQualifiedName,
            });
      });
    }

    [TestMethod]
    public void TestStaticClass()
    {
      RunIsolated(() =>
      {
        StaticClassA.DoIt();
        ValidateEvents(new string[] { typeof(StaticClassA).Name });
      });
    }

    #endregion
  }
}
