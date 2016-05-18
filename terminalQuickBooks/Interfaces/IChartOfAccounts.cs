using System.Collections.Generic;
using Fr8Data.Manifests;
using Intuit.Ipp.Data;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

namespace terminalQuickBooks.Interfaces
{
    public interface IChartOfAccounts
    {
        ChartOfAccountsCM GetChartOfAccounts(AuthorizationToken authorizationToken, string userId, IHubCommunicator hubCommunicator);
    }
}