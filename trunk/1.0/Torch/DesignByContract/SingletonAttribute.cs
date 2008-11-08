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
    public class SingletonAttribute : Attribute, IRequirePostSharp
    {
        #region IRequirePostSharp Members

        public PostSharpRequirements GetPostSharpRequirements()
        {
            PostSharpRequirements requirements = new PostSharpRequirements();
            requirements.PlugIns.Add("Torch.DesignByContract");
            requirements.Tasks.Add("Torch.DesignByContract.CheckSingleton");
            return requirements;
        }

        #endregion
    }
}
