using System.Collections.Generic;
using Data.Entities;
using Fr8Data.Manifests;
using Intuit.Ipp.Data;

namespace terminalQuickBooks.Interfaces
{
    public interface IChartOfAccounts
    {
        ChartOfAccountsCM GetChartOfAccounts(AuthorizationTokenDO authTokenDO, string userId);
    }
}