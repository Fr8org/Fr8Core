using System.Collections.Generic;
using Data.Entities;
using Fr8Data.Manifests;
using Intuit.Ipp.Data;
using TerminalBase.Infrastructure;

namespace terminalQuickBooks.Interfaces
{
    public interface IChartOfAccounts
    {
        ChartOfAccountsCM GetChartOfAccounts(AuthorizationTokenDO authTokenDO, string userId, IHubCommunicator hubCommunicator);
    }
}