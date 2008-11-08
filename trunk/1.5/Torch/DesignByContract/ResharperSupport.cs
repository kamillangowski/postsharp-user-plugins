
using System;
using System.Collections.Generic;
using Torch.DesignByContract;

namespace JetBrains.Annotations
{

    /// <summary>
    /// Indicates that the value of marked element could never be <c>null</c>
    /// </summary>
    public sealed class NotNullAttribute : NonNullAttribute
    {
    }

}
