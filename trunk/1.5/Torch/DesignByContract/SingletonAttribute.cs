using System;
using PostSharp.Extensibility;

namespace Torch.DesignByContract
{
    // TODO: se puede heredar?
    [AttributeUsage(
        AttributeTargets.Class,
        Inherited = true,
        AllowMultiple = false)
    ]
    [RequirePostSharp("Torch.DesignByContract", "Torch.DesignByContract.CheckSingleton")]
    public class SingletonAttribute : Attribute
    {
    }
}