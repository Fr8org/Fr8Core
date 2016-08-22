using System;

namespace terminalStatX.Infrastructure
{
    /// <summary>
    /// Mark all properties that need to be rendered on the UI and ready for update with this attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class RenderUiPropertyAttribute : Attribute
    {
    }
}