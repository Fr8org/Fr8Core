using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json.Linq;
using terminalAsana.Asana;

namespace terminalAsana.Interfaces
{
    public interface IAsanaOAuth
    {
        AuthorizationToken  AuthorizationToken { get; }
        OAuthToken          OAuthToken { get; set; }

        bool                IsTokenExpired();
        bool                IsTokenExpired(OAuthToken token);
        Task<OAuthToken>    RefreshOAuthTokenAsync();
        Task<OAuthToken>    RefreshOAuthTokenAsync(OAuthToken token);
        Task<OAuthToken>    RefreshTokenIfExpiredAsync();
        Task<OAuthToken>    RefreshTokenIfExpiredAsync(OAuthToken token);

        DateTime            CalculateExpirationTime(int secondsToExpiration);
        string              CreateAuthUrl(string state);
        Task<JObject>       GetOAuthTokenDataAsync(string code);

        bool                IsIntialized { get; }
        Task<IAsanaOAuth>   InitializeAsync(AuthorizationToken authorizationToken, IHubCommunicator hubCommunicator);
    }
}