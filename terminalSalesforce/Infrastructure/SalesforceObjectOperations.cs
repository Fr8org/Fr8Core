using System;

namespace terminalSalesforce.Infrastructure
{
    [Flags]
    public enum SalesforceObjectOperations
    {
        None = 0,
        Create = 1,
        All = Create
    }
}