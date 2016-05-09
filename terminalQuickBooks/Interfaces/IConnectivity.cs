using System.Threading.Tasks;
using Data.Entities;
using Intuit.Ipp.Core;
using Intuit.Ipp.DataService;

namespace terminalQuickBooks.Interfaces
{
    public interface IConnectivity
    {
        string CreateAuthUrl();
        DataService GetDataService(AuthorizationTokenDO authTokenDO);
        Task<string> GetOAuthToken(string oauthToken, string oauthVerifier, string realmId);
        ServiceContext CreateServiceContext(string oauthToken);
    }
}