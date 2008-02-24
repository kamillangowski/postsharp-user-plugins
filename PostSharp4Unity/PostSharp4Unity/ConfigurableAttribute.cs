using System;
using Microsoft.Practices.Unity;
using PostSharp;
using PostSharp.Laos;

namespace PostSharp4Unity
{
    /// <summary>
    /// When applied on a type (eventually using multicasting), enhances the 
    /// object constructor so that it is 'built up' from the current container.
    /// </summary>
    /// <remarks>
    /// <para>When a class is decorated with this custom attribute, it can
    /// be created using normal constructors and yet configured by the current
    /// Unity container.</para>
    /// <para>Since Unity has no notion of 'current' container, you have to
    /// provide your own by implementing the <see cref="IUnityContainerProvider"/>
    /// interface. You can specify an assembly-wide container provider
    /// by applying the <see cref="DefaultUnityContainerProviderAttribute"/>
    /// custom attribute on your assembly. Alternatively, you can specify
    /// a container provider for each class using the proper constructor
    /// <see cref="ConfigurableAttribute(Type)"/>.</para>
    /// <para><b>Warning.</b> When an object is marked as configurable,
    /// it is configured <i>before</i> the constructor is executed. However,
    /// when you construct an object using the normal Unity factory method,
    /// the object is configured <i>after</i> the constructor is executed.</para>
    /// <para><b>Note.</b> This custom attribute has no effect if it is applied
    /// on a class that derives from a parent implementing the <see cref="IConfigurable"/>
    /// interface or decored with the <see cref="ConfigurableAttribute"/>
    /// custom attribute.</para>
    /// <para><b>Note.</b> The interface <see cref="IConfigurable"/> will be
    /// introduced after compilation. You can use the method <see cref="Post.Cast{SourceType,TargetType}"/>
    /// to cast an instance of the target class to <see cref="IConfigurable"/>.
    /// </para>
    /// </remarks>
    [Serializable]
    public sealed class ConfigurableAttribute : CompositionAspect
    {
        private readonly string unityContainerProviderType;

        [NonSerialized] private IUnityContainerProvider containerProvider;

        /// <summary>
        /// Marks the class as Unity-configurable and uses the container provider
        /// specified by the <see cref="DefaultUnityContainerProviderAttribute"/>
        /// in the current assembly.
        /// </summary>
        public ConfigurableAttribute()
        {
        }

        /// <summary>
        /// Marks the class as Unity-configurable and specify the container provider
        /// type.
        /// </summary>
        /// <param name="unityContainerProviderType">Type implementing the
        /// <see cref="IUnityContainerProvider"/> interface. This type should have
        /// a default public constructor. A singleton instance of this type
        /// will be created.</param>
        public ConfigurableAttribute(Type unityContainerProviderType)
        {
            this.unityContainerProviderType = unityContainerProviderType.AssemblyQualifiedName;
        }

        /// <summary>
        /// Invoked at runtime at the entry of the constructor of the class to which
        /// this custom attribute is applied.
        /// </summary>
        /// <param name="eventArgs">Context information.</param>
        /// <returns>The implementation of the <see cref="IConfigurable"/>
        /// interface. </returns>
        public override object CreateImplementationObject(InstanceBoundLaosEventArgs eventArgs)
        {
            // Check that we got a container provider.
            if (this.containerProvider == null)
            {
                throw new InvalidOperationException(
                    "No container provider has been defined. Use the DefaultUnityContainerProviderAttribute custom attribute to specify an assembly-wide provider, or specify it in the constructor of the ConfigurableAttribute custom attribute.");
            }

            // Initialize the object.
            this.containerProvider.CurrentContainer.BuildUp(eventArgs.Instance.GetType(), eventArgs.Instance);

            // Create the implementation of IConfigurable and return it.
            return new ConfigurableImpl(this.containerProvider.CurrentContainer);
        }

        /// <summary>
        /// Invoked at compile-time. Gets the public interface to be implemented.
        /// </summary>
        /// <param name="containerType">Type to which the current custom
        /// attribute instance is applied.</param>
        /// <returns>Always the interface <see cref="IConfigurable"/>.</returns>
        public override Type GetPublicInterface(Type containerType)
        {
            return typeof (IConfigurable);
        }

        /// <summary>
        /// Invoked at run-time when the current custom attribute instance
        /// is initialized. We get a reference to the container provider.
        /// </summary>
        /// <param name="type">Type to which the current instance of the
        /// custom attribute is applied.</param>
        public override void RuntimeInitialize(Type type)
        {
            base.RuntimeInitialize(type);

            // First get the provider type.
            Type providerType;
            if (!string.IsNullOrEmpty(this.unityContainerProviderType))
            {
                // The provider type was specified in the custom attribute instance.
                providerType = Type.GetType(this.unityContainerProviderType);
            }
            else
            {
                // The provider type was not specified. Get the default provider of the
                // assembly declaring the type to which the current custom attribute
                // instance is applied.

                DefaultUnityContainerProviderAttribute[] attributes = (DefaultUnityContainerProviderAttribute[])
                                                                      type.Assembly.GetCustomAttributes(
                                                                          typeof (
                                                                              DefaultUnityContainerProviderAttribute
                                                                              ), false);

                if (attributes.Length > 0)
                {
                    providerType = attributes[0].Type;
                }
                else
                {
                    // This is an error: the user should define a DefaultUnityContainerProviderAttribute.
                    // However, since we are in a static constructor, and since exceptions
                    // in static constructors are difficult to diagnose, we do not
                    // throw an exception immediately. We will throw it when an instance
                    // of our target object will effectively be created.
                    providerType = null;
                }
            }

            // Get a singleton instance of our provider.
            if (providerType != null)
            {
                this.containerProvider = UnityContainerProviderFactory.GetIntance(providerType);
            }
        }


        /// <summary>
        /// Implementation of <see cref="IConfigurable"/>.
        /// </summary>
        private sealed class ConfigurableImpl : IConfigurable
        {
            private readonly IUnityContainer container;

            public ConfigurableImpl(IUnityContainer container)
            {
                this.container = container;
            }

            public IUnityContainer Container
            {
                get { return this.container; }
            }
        }
    }
}