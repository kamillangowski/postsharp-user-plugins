using System;
using System.Diagnostics;
using log4net;
using log4net.Appender;
using log4net.Core;
using Log4PostSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PostSharp.Extensibility;

[assembly: LoggerPolicy(AttributeTargetElements = MulticastTargets.Class,
  AttributeTargetTypes = "Log4PostSharp.Test.HelperClass*",
  LoggerNamePolicy = LoggerNamePolicy.AssemblyQualifiedName,
  GenericArgsLoggerNamePolicy = LoggerNamePolicy.FullTypeName)]
[assembly: LoggerPolicy(AttributeTargetElements = MulticastTargets.Class,
  AttributeTargetTypes = "Log4PostSharp.Test.LoggerPolicyAttributeTest+InternalClass*",
  LoggerNamePolicy = LoggerNamePolicy.AssemblyQualifiedName,
  GenericArgsLoggerNamePolicy = LoggerNamePolicy.TypeName)]
[assembly: LoggerPolicy(AttributeTargetElements = MulticastTargets.Class,
  AttributeTargetTypes = "Log4PostSharp.Test.NoLogAttributeClass",
  LoggerNamePolicy = LoggerNamePolicy.AssemblyQualifiedName,
  GenericArgsLoggerNamePolicy = LoggerNamePolicy.TypeName)]
namespace Log4PostSharp.Test
{
  #region Helper Classes

  [Log]
  public class HelperClassA { }

  [Log]
  public class OutOfPolicyClass { }

  [Log]
  public class HelperClassB<T> { }

  [Log]
  [Logger("LoggerPolicyHelper")]
  public class HelperClassB { }

  [Log]
  [Logger("LoggerPolicyHelper")]
  public class CollidingClass { }

  public class NoLogAttributeClass { }

  #endregion

  /// <summary>
  ///This is a test class for LoggerPolicyAttributeTest and is intended
  ///to contain all LoggerPolicyAttributeTest Unit Tests
  ///</summary>
  [TestClass()]
  public class LoggerPolicyAttributeTest
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
    public class InternalClassA { }

    [Log]
    public class InternalClassB<T> { }

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
      AppenderHolder.Appender.Clear();
      LoggerHelper.ReportNameCollisions = false;
    }

    #endregion

    #region Test Methods

    [TestMethod]
    public void TestSimpleClass()
    {
      new HelperClassA();
      ValidateEvents(new string[] { typeof(HelperClassA).AssemblyQualifiedName });
    }

    [TestMethod]
    public void TestGenericClass()
    {
      new HelperClassB<int>();
      ValidateEvents(new string[]{typeof(HelperClassB<int>).AssemblyQualifiedName
        + "<" + typeof(int).FullName +">"});
    }

    [TestMethod]
    public void TestGenericClassWithGenericArgsFitsToPolicy()
    {
      //HelperClassA fits the policy, hence we're expecting to see its assembly qualified name and not
      //its full type name.
      new HelperClassA();
      new HelperClassB<HelperClassA>();
      ValidateEvents(new string[]{typeof(HelperClassA).AssemblyQualifiedName, 
        typeof(HelperClassB<HelperClassA>).AssemblyQualifiedName + "<" + typeof(HelperClassA).AssemblyQualifiedName +">"});
    }

    [TestMethod]
    public void TestOutofLoggerPolicyClass()
    {
      new OutOfPolicyClass();
      ValidateEvents(new string[] { typeof(OutOfPolicyClass).FullName }); //LoggerHelper.DefaultLoggerPolicy
    }

    [TestMethod]
    public void TestLoggerAttributeOverride()
    {
      new HelperClassB();
      ValidateEvents(new string[] { "LoggerPolicyHelper" });
    }

    [TestMethod]
    public void TestNestedClasses()
    {
      new InternalClassA();
      new InternalClassB<int>();

      ValidateEvents(new string[] { 
        typeof(InternalClassA).AssemblyQualifiedName, 
        typeof(InternalClassB<int>).AssemblyQualifiedName + "<" + typeof(int).Name +">" 
      });
    }

    [TestMethod]
    public void TestClassWithNoLogAttribute()
    {
      new NoLogAttributeClass();
      LoggerHelper.GetLogger(typeof(NoLogAttributeClass)).Info("Testing");

      ValidateEvents(new string[] { typeof(NoLogAttributeClass).AssemblyQualifiedName });
    }

    [TestMethod]
    public void TestCollidingNameFitsToPolicy()
    {
      RunIsolated(() =>
      {
        LoggerHelper.ReportNameCollisions = false;

        new CollidingClass();
        new HelperClassB(); //Colliding with CollidingClass and fits the broadcast policy.
        //Using the internal LoggerHelper fallback policy in this case (AssemblyQualifiedName).

        ValidateEvents(new string[] {
          "LoggerPolicyHelper",
          typeof(HelperClassB).AssemblyQualifiedName
        });
      });
    }

    #endregion
  }
}
