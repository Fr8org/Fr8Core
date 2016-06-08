using System.Threading.Tasks;
using fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Models;

namespace terminalQuickBooks.Interfaces
{
    public interface IAuthenticator
    {
        string CreateAuthUrl();
        Task<AuthorizationTokenDTO> GetAuthToken(string oauthToken, string oauthVerifier, string realmId);
        Task<AuthorizationToken> RefreshAuthToken(AuthorizationToken curAuthTokenDO);
    }
}