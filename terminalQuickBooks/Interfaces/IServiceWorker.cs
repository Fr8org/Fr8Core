using System.Threading.Tasks;
using Data.Entities;
using Intuit.Ipp.Core;
using Intuit.Ipp.DataService;

namespace terminalQuickBooks.Interfaces
{
    public interface IServiceWorker
    {
        DataService GetDataService(AuthorizationTokenDO authTokenDO, string userId);
    }
}