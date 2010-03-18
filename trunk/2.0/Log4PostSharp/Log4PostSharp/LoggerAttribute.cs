using System;
using PostSharp.Extensibility;
using log4net;

namespace Log4PostSharp
{
  /// <summary>
  /// This attribute contains the information about an <see cref="ILog"/> used by specific class.
  /// </summary>
  /// <remarks>
  /// When applying this attribute on a <see cref="Type"/>, use <see cref="LoggerHelper.GetLogger"/> method to retrieve
  /// this type's <see cref="ILog"/> during run time.
  /// <para>
  /// If two types declare on the same logger name using <see cref="LoggerAttribute"/>,
  /// the first type whose static constructor will be called will have the declared name, the second type will get its <see cref="Type.AssemblyQualifiedName"/>
  /// as the logger name.
  /// </para> 
  /// </remarks>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
  [RequirePostSharp("Log4PostSharp", "Log4PostSharp.Weaver.LogTask")]
  public class LoggerAttribute : Attribute
  {
    #region Private Fields

    private string m_name;
    private LoggerNamePolicy m_namePolicy;
    private LoggerNamePolicy m_genericArgsNamePolicy;

    #endregion

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerAttribute"/> with an explicit logger name for the class.
    /// </summary>
    /// <param name="name">The name of <see cref="ILog"/>.</param>
    public LoggerAttribute(string name)
    {
      m_name = name;
      m_genericArgsNamePolicy = LoggerHelper.DefaultLoggerNamePolicy;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerAttribute"/> with a <see cref="LoggerNamePolicy"/> for the class.
    /// </summary>
    /// <param name="namePolicy">The <see cref="LoggerNamePolicy"/> to be used.</param>
    public LoggerAttribute(LoggerNamePolicy namePolicy)
    {
      m_namePolicy = namePolicy;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerAttribute"/> with an explicit logger name and <see cref="LoggerNamePolicy"/> for
    /// any generic arguments of the class.
    /// </summary>
    /// <param name="name">The name of <see cref="ILog"/>.</param>
    /// <param name="genericArgsNamePolicy">The <see cref="LoggerNamePolicy"/> to be used for any generic argument.</param>
    /// <remarks>
    /// The <paramref name="genericArgsNamePolicy"/> is a fall back policy. If any of the generic arguments types
    /// owns its own <see cref="LoggerAttribute"/> it will be used.
    /// </remarks>
    public LoggerAttribute(string name, LoggerNamePolicy genericArgsNamePolicy)
      : this(name)
    {
      m_genericArgsNamePolicy = genericArgsNamePolicy;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerAttribute"/> with <see cref="LoggerNamePolicy"/> for
    /// the type and any generic arguments.
    /// </summary>
    /// <param name="namePolicy">The <see cref="LoggerNamePolicy"/> to be used for the type.</param>
    /// <param name="genericArgsNamePolicy">The <see cref="LoggerNamePolicy"/> to be used for any generic argument.</param>
    /// <remarks>
    /// The <paramref name="genericArgsNamePolicy"/> is a fall back policy. If any of the generic arguments types
    /// owns its own <see cref="LoggerAttribute"/> it will be used.
    /// </remarks>
    public LoggerAttribute(LoggerNamePolicy namePolicy, LoggerNamePolicy genericArgsNamePolicy)
    {
      m_namePolicy = namePolicy;
      m_genericArgsNamePolicy = genericArgsNamePolicy;
    }


    #endregion

    #region Internal Properties

    /// <summary>
    /// Gets the name for the <see cref="ILog"/> of the type.
    /// </summary>
    public string Name
    {
      get { return m_name; }
    }

    /// <summary>
    /// Gets the <see cref="LoggerNamePolicy"/> for the type.
    /// </summary>
    public LoggerNamePolicy LoggerNamePolicy
    {
      get { return m_namePolicy; }
    }

    /// <summary>
    /// Gets the <see cref="LoggerNamePolicy"/> for the generic args of the type.
    /// </summary>
    public LoggerNamePolicy GenericArgsNamePolicy
    {
      get { return m_genericArgsNamePolicy; }
    }

    #endregion
  }
}
