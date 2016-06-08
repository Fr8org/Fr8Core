using fr8.Infrastructure.Data.Manifests;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

namespace terminalQuickBooks.Interfaces
{
    public interface IChartOfAccounts
    {
        ChartOfAccountsCM GetChartOfAccounts(AuthorizationToken authorizationToken,  IHubCommunicator hubCommunicator);
    }
}