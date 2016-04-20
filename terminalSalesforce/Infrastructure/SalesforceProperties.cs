using System;

namespace terminalSalesforce.Infrastructure
{
    [Flags]
    public enum SalesforceProperties
    {
        None = 0,
        HasChatter = 1,
        All = HasChatter    
    }
}