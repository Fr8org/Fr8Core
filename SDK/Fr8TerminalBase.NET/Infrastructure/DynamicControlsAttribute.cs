using System;

namespace Fr8.TerminalBase.Infrastructure
{
    // Mark collection property in your ActivityUi class to enable this collection to sync with dynamic controls list
    // See https://maginot.atlassian.net/wiki/display/DDW/Support+for+dynamic+controls+in+ETA for more details
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DynamicControlsAttribute : Attribute
    {
    }
}