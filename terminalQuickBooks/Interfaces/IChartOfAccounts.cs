using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;

namespace terminalQuickBooks.Interfaces
{
    public interface IChartOfAccounts
    {
        ChartOfAccountsCM GetChartOfAccounts(AuthorizationToken authorizationToken,  IHubCommunicator hubCommunicator, IServiceWorker serviceWorker);
    }
}