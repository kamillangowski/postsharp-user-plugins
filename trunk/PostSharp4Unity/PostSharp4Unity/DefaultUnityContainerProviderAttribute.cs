using System;

namespace PostSharp4Unity
{
    /// <summary>
    /// Custom attribute that, when applied on an assembly, specifies the default
    /// Unity container provider (an implementation of <see cref="IUnityContainerProvider"/>)
    /// for all Unity-configurable types defined in that assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class DefaultUnityContainerProviderAttribute : Attribute
    {
        private readonly Type type;

        /// <summary>
        /// Specifies the default Unity container provider
        /// for all Unity-configurable types defined in the current assembly.
        /// </summary>
        /// <param name="type">An implementation of <see cref="IUnityContainerProvider"/>. This
        /// class should have a default public constructor. A singleton instance of this
        /// class will be created at runtime.</param>
        public DefaultUnityContainerProviderAttribute(Type type)
        {
            this.type = type;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> implementing <see cref="IConfigurable"/>.
        /// </summary>
        public Type Type
        {
            get { return type; }
        }
    }
}