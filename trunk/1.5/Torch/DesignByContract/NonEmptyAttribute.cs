using System;
using PostSharp.Extensibility;

namespace Torch.DesignByContract
{
    [AttributeUsage(
        AttributeTargets.GenericParameter | AttributeTargets.Parameter | AttributeTargets.ReturnValue,
        Inherited = true,
        AllowMultiple = false)]
    [MulticastAttributeUsage(MulticastTargets.Parameter | MulticastTargets.ReturnValue, AllowMultiple = false,
        TargetMemberAttributes = MulticastAttributes.NonAbstract, Inheritance = MulticastInheritance.Strict,
        PersistMetaData = true)]
    [RequirePostSharp("Torch.DesignByContract", "Torch.DesignByContract.CheckNonEmpty")]
    public class NonEmptyAttribute : MulticastAttribute
    {
    }
}