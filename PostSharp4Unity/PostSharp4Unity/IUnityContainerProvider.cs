using Microsoft.Practices.Unity;

namespace PostSharp4Unity
{
    /// <summary>
    /// Provides the Unity container in the current context.
    /// </summary>
    /// <remarks>
    /// <para>This interface exposes the minimalist semantics of a context registry.
    /// You should have at least one Unity container provider in your application.
    /// In easiest case, it is enough to serve a singleton instance of <see cref="UnityContainer"/>.
    /// </para>
    /// <para>
    /// Use the <see cref="DefaultUnityContainerProviderAttribute"/> custom attribute
    /// to specify the assembly-wide provider. Alternatively, you can specify an 
    /// implementation in each construction of a <see cref="ConfigurableAttribute"/>
    /// attribute instance.
    /// </para>
    /// </remarks>
    public interface IUnityContainerProvider
    {
        /// <summary>
        /// Gets the Unity container for the current context.
        /// </summary>
        IUnityContainer CurrentContainer { get; }
    }
}