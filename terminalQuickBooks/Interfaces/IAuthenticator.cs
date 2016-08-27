using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Models;

namespace terminalQuickBooks.Interfaces
{
    public interface IAuthenticator
    {
        string CreateAuthUrl(Guid state);
        Task<AuthorizationTokenDTO> GetAuthToken(string oauthToken, string oauthVerifier, string realmId, string state);
        Task<AuthorizationToken> RefreshAuthToken(AuthorizationToken curAuthTokenDO);
    }
}