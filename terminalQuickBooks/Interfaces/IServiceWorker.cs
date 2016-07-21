using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Intuit.Ipp.DataService;

namespace terminalQuickBooks.Interfaces
{
    public interface IServiceWorker
    {
        DataService GetDataService(AuthorizationToken authToken, IHubCommunicator hubCommunicator);
    }
}