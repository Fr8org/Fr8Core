using Intuit.Ipp.DataService;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

namespace terminalQuickBooks.Interfaces
{
    public interface IServiceWorker
    {
        DataService GetDataService(AuthorizationToken authToken, IHubCommunicator hubCommunicator);
    }
}