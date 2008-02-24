using System;
using System.Collections.Generic;

namespace PostSharp4Unity
{
    /// <summary>
    /// Manages singleton instances of <see cref="IUnityContainerProvider"/>.
    /// </summary>
    internal static class UnityContainerProviderFactory
    {
        private static readonly Dictionary<Type, IUnityContainerProvider> instances =
            new Dictionary<Type, IUnityContainerProvider>();

        public static IUnityContainerProvider GetIntance(Type type)
        {
            IUnityContainerProvider instance;

            if (!instances.TryGetValue(type, out instance))
            {
                lock (instances)
                {
                    if (!instances.TryGetValue(type, out instance))
                    {
                        instance = (IUnityContainerProvider) Activator.CreateInstance(type);
                        instances.Add(type, instance);
                    }
                }
            }

            return instance;
        }
    }
}