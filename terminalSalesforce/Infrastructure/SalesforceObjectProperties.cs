using System;

namespace terminalSalesforce.Infrastructure
{
    [Flags]
    public enum SalesforceObjectProperties
    {
        None = 0,
        HasChatter = 1,
        All = HasChatter    
    }
}