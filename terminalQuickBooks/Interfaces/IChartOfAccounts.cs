using System.Collections.Generic;
using Data.Entities;
using Fr8Data.Manifests;
using Intuit.Ipp.Data;

namespace terminalQuickBooks.Interfaces
{
    public interface IChartOfAccounts
    {
        List<Account> GetAccountList(AuthorizationTokenDO authTokenDO);
        ChartOfAccountsCM GetChartOfAccounts(AuthorizationTokenDO authTokenDO);
    }
}