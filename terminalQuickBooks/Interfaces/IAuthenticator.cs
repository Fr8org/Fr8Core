using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Fr8Data.DataTransferObjects;

namespace terminalQuickBooks.Interfaces
{
    public interface IAuthenticator
    {
        string CreateAuthUrl();
        Task<AuthorizationTokenDTO> GetAuthToken(string oauthToken, string oauthVerifier, string realmId);
        Task<AuthorizationTokenDO> RefreshAuthToken(AuthorizationTokenDO curAuthTokenDO);
    }
}