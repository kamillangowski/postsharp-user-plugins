using System;
using PostSharp.Extensibility;

namespace Torch.DesignByContract
{
    [AttributeUsage(
        AttributeTargets.GenericParameter | AttributeTargets.Parameter | AttributeTargets.ReturnValue ,
        Inherited = true,
        AllowMultiple = false)
    ]
    public class NonNullAttribute : Attribute, IRequirePostSharp
    {
        #region IRequirePostSharp Members

        public PostSharpRequirements GetPostSharpRequirements()
        {
            PostSharpRequirements requirements = new PostSharpRequirements();
            requirements.PlugIns.Add("Torch.DesignByContract");
            requirements.Tasks.Add("Torch.DesignByContract.CheckNonNull");
            return requirements;
        }

        #endregion
    }
}
