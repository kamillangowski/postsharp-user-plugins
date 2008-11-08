using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.Unity;
using PostSharp4Unity;
using StopLight;
using StopLight.ServiceImplementations;
using StopLight.ServiceInterfaces;

[assembly: DefaultUnityContainerProvider(typeof(UnityContainerProvider))]

namespace StopLight
{
    public sealed class UnityContainerProvider : IUnityContainerProvider
    {
        private readonly IUnityContainer container;

        public UnityContainerProvider()
        {
            this.container = new UnityContainer()
            .Register<ILogger, TraceLogger>()
            .Register<IStoplightTimer, RealTimeTimer>();

        }
        public IUnityContainer CurrentContainer
        {
            get { return this.container; }
        }
    }
}
