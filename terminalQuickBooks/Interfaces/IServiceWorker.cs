using System.Threading.Tasks;
using Intuit.Ipp.Core;
using Intuit.Ipp.DataService;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

namespace terminalQuickBooks.Interfaces
{
    public interface IServiceWorker
    {
        DataService GetDataService(AuthorizationToken authToken, string userId, IHubCommunicator hubCommunicator);
    }
}