using Microsoft.Practices.Unity;
using PostSharp;

namespace PostSharp4Unity
{
    /// <summary>
    /// Interface introduced into types decored with the <see cref="ConfigurableAttribute"/>
    /// custom attribute. It provides basically a reference to the container that was
    /// used to build the object.
    /// </summary>
    /// <remarks>
    /// This interface is introduced after compilation. To cast an object to this
    /// interface, use the method <see cref="Post.Cast{SourceType,TargetType}"/>.
    /// </remarks>
    public interface IConfigurable
    {
        /// <summary>
        /// Gets the container that was used to build the object.
        /// </summary>
        IUnityContainer Container { get; }
    }
}