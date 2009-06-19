using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using log4net;

namespace Log4PostSharp
{
  /// <summary>
  /// Contains logging related utility methods. <see cref="LoggerHelper"/> as well manages registration
  /// of class types and their logger names. The registration is based on the logic provided by <see cref="LoggerAttribute"/>
  /// for each class.
  /// </summary>
  public static class LoggerHelper
  {
    #region Private Fields

    private static IDictionary<Type, string> sm_loggerNames = new Dictionary<Type, string>();
    private static bool sm_reportNameCollisions = true;

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets instance of <see cref="ILog"/> for certain <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> whose <see cref="ILog"/> name should be retrieved.</param>
    /// <returns>Instance of <see cref="ILog"/>.</returns>
    public static ILog GetLogger(Type type)
    {
      if (type == null)
      {
        Debug.Fail("type == null");
        throw new ArgumentNullException("type");
      }

      string loggerName;
      if (sm_loggerNames.TryGetValue(type, out loggerName))
      {
        return LogManager.GetLogger(loggerName);
      }

      return LogManager.GetLogger(type);
    }

    /// <summary>
    /// Generates a name for <see cref="ILog"/> for specific <see cref="Type"/> and registeres this name for this type's
    /// logger usage only.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> whose <see cref="ILog"/> name should be regsitered.</param>
    /// <returns>The name of the <see cref="ILog"/>.</returns>
    /// <remarks>
    /// The locig behind logger's name generation relies on <see cref="LoggerAttribute"/> attached to the class.
    /// If no such attribute attached, the <see cref="LoggerHelper.DefaultLoggerNamePolicy"/> will be applied.
    /// </remarks>
    public static string RegisterLogger(Type type)
    {
      if (type == null)
      {
        Debug.Fail("type == null");
        throw new ArgumentNullException("type");
      }

      return RegisterLoggerInternal(type, (t) =>
        {
          LoggerAttribute[] atts = (LoggerAttribute[])t.GetCustomAttributes(typeof(LoggerAttribute), false);
          return atts.Length > 0 ? atts[0] : null;
        });
    }

    /// <summary>
    /// Generates a name for <see cref="ILog"/> for specific <see cref="Type"/> and registeres this name for this type's
    /// logger usage only. The name is generated according to the given logger name policies.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> whose <see cref="ILog"/> name should be regsitered.</param>
    /// <param name="loggerNamePolicy">The <see cref="LoggerNamePolicy"/> to be applied on the name of the class.</param>
    /// <param name="genericArgsLoggerNamePolicy">The <see cref="LoggerNamePolicy"/> to be applied on the name of the generic arguments of class.</param>
    /// <returns>The name of the <see cref="ILog"/>.</returns>
    /// <remarks>
    /// If <paramref name="type"/> has a <see cref="LoggerAttribute"/> attached to it, the logic within the attribute will override 
    /// <paramref name="loggerNamePolicy"/> and <paramref name="genericArgsLoggerNamePolicy"/> parameters.
    /// </remarks>
    public static string RegisterLogger(Type type,
      LoggerNamePolicy loggerNamePolicy,
      LoggerNamePolicy genericArgsLoggerNamePolicy)
    {
      if (type == null)
      {
        Debug.Fail("type == null");
        throw new ArgumentNullException("type");
      }

      return RegisterLoggerInternal(type, (t) =>
        {
          LoggerAttribute[] atts = (LoggerAttribute[])t.GetCustomAttributes(typeof(LoggerAttribute), false);
          LoggerAttribute log = atts.Length > 0 ? atts[0] : null;

          //If the type contains its own LoggerAttribute, it will have the precedence.
          return log ?? new LoggerAttribute(loggerNamePolicy, genericArgsLoggerNamePolicy);
        });
    }

    #endregion

    #region Public Fields

    /// <summary>
    /// Default <see cref="LoggerNamePolicy"/> to be used if not specified explicitly by the <see cref="LoggerAttribute"/>.
    /// </summary>
    public static readonly LoggerNamePolicy DefaultLoggerNamePolicy = LoggerNamePolicy.FullTypeName;

    /// <summary>
    /// Indicates whether logger name collisions should be reported into the log.
    /// </summary>
    public static bool ReportNameCollisions
    {
      get { return sm_reportNameCollisions; }
      set { sm_reportNameCollisions = value; }
    }

    #endregion

    #region Private Helper Methods

    private static string RegisterLoggerInternal(Type type, Func<Type, LoggerAttribute> del)
    {
      if (type == null)
      {
        Debug.Fail("type == null");
        throw new ArgumentNullException("type");
      }

      string loggerName;
      //Check if already regsitered.
      if (!sm_loggerNames.TryGetValue(type, out loggerName))
      {
        //If not, generate a new name according to logic specified by the LoggerAttribute.
        StringBuilder accumulator = new StringBuilder();
        GetLoggerName(type, del, DefaultLoggerNamePolicy, accumulator);
        loggerName = accumulator.ToString();

        //Check for logger name collisions. If there is a collision the name of the logger of the
        //type will be type's assembly qualified name.
        if (sm_loggerNames.Values.Contains(loggerName))
        {
          loggerName = type.AssemblyQualifiedName;
          ReportCollision(type);
        }

        //Register the new generated name.
        sm_loggerNames[type] = loggerName;
      }

      return loggerName;
    }

    private static void GetLoggerName(Type type, Func<Type, LoggerAttribute> attFunc, LoggerNamePolicy fallbackPolicy, StringBuilder nameAccumulator)
    {
      LoggerAttribute log = attFunc(type);

      string loggerName = ApplyLoggerNamePolicy(type,
        log != null ? log.Name : string.Empty,
        log != null ? log.LoggerNamePolicy : fallbackPolicy);

      nameAccumulator.Append(loggerName);

      if (type.IsGenericType)
      {
        string delim = "<";
        LoggerNamePolicy genericArgsPolicy = log != null ? log.GenericArgsNamePolicy : fallbackPolicy;

        foreach (Type genericArg in type.GetGenericArguments())
        {
          //Check if the logger name for this type has already been registered.
          string gerericTypeName;
          if (sm_loggerNames.TryGetValue(genericArg, out gerericTypeName))
          {
            nameAccumulator.Append("<" + gerericTypeName + ">");
            return;
          }

          nameAccumulator.Append(delim);
          GetLoggerName(genericArg, (t) =>
                                    {
                                      LoggerAttribute[] atts = (LoggerAttribute[])t.GetCustomAttributes(typeof(LoggerAttribute), false);
                                      return atts.Length > 0 ? atts[0] : null;
                                    }
                                   , genericArgsPolicy, nameAccumulator);
          delim = ",";
        }

        nameAccumulator.Append(">");
      }
    }

    private static string ApplyLoggerNamePolicy(Type type, string customName, LoggerNamePolicy policy)
    {
      if (!string.IsNullOrEmpty(customName))
      {
        return customName;
      }

      switch (policy)
      {
      case LoggerNamePolicy.TypeName: return type.Name;
      case LoggerNamePolicy.FullTypeName: return type.FullName;
      case LoggerNamePolicy.AssemblyQualifiedName: return type.AssemblyQualifiedName;
      }

      Debug.Fail("Unsupported LoggerNamePolicy type");
      throw new ArgumentException("Unsupported LoggerNamePolicy type");
    }

    private static void ReportCollision(Type type)
    {
      if (!sm_reportNameCollisions)
      {
        return;
      }

      string message = string.Format("Logger name collision occured while registering type <{0}>, using assembly qualified name.", type.FullName.ToString());
      Log.Info(message);
    }

    private static ILog Log
    {
      get { return LogManager.GetLogger("LoggerHelper"); }
    }

    #endregion
  }


  /// <summary>
  /// Defines set of policies of creating names of <see cref="ILog"/> for variouse types, used by <see cref="LoggerAttribute"/>.
  /// </summary>
  public enum LoggerNamePolicy
  {
    /// <summary>
    /// The name of the <see cref="ILog"/> for the <see cref="Type"/> will be <see cref="System.Reflection.MemberInfo.Name"/>.
    /// </summary>
    TypeName,
    /// <summary>
    /// The name of the <see cref="ILog"/> for the <see cref="Type"/> will be <see cref="Type.FullName"/>.
    /// </summary>
    FullTypeName,
    /// <summary>
    /// The name of the <see cref="ILog"/> for the <see cref="Type"/> will be <see cref="Type.AssemblyQualifiedName"/>.
    /// </summary>
    AssemblyQualifiedName,
  }
}
