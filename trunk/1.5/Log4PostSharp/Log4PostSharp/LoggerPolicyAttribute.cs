using System;
using PostSharp.Extensibility;

namespace Log4PostSharp
{
  /// <summary>
  /// Broadcasts logger name policy for logable classes under certain assembly. A class will be eligible for this attribute if it contains at least 
  /// one instance of <see cref="LogAttribute"/> in its metadata, or fits the broadcasting criteria specified by <see cref="LoggerPolicyAttribute"/>.
  /// </summary>
  /// <remarks>
  /// Policy specified by <see cref="LoggerPolicyAttribute"/> can be overriden by <see cref="LoggerAttribute"/>.
  /// </remarks>
  /// <seealso cref="PostSharp.Extensibility.MulticastAttribute"/>
  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
  [MulticastAttributeUsage(MulticastTargets.Class, AllowMultiple = true)]
  [Serializable]
  [RequirePostSharp("Log4PostSharp", "Log4PostSharp.Weaver.LogTask")]
  public class LoggerPolicyAttribute : MulticastAttribute
  {
    #region Private Fields

    private LoggerNamePolicy m_namePolicy = LoggerHelper.DefaultLoggerNamePolicy;
    private LoggerNamePolicy m_genericArgsNamePolicy = LoggerHelper.DefaultLoggerNamePolicy;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets and sets the <see cref="LoggerNamePolicy"/> to be applied on the class type.
    /// </summary>
    public LoggerNamePolicy LoggerNamePolicy
    {
      get { return m_namePolicy; }
      set { m_namePolicy = value; }
    }

    /// <summary>
    /// Gets and sets the <see cref="LoggerNamePolicy"/> to be applied on the generic arguments of the class type.
    /// </summary>
    public LoggerNamePolicy GenericArgsLoggerNamePolicy
    {
      get { return m_genericArgsNamePolicy; }
      set { m_genericArgsNamePolicy = value; }
    }

    #endregion
  }
}
