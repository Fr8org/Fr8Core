using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json.Linq;
using terminalAsana.Asana;
using terminalAsana.Asana.Services;

namespace terminalAsana.Interfaces
{
    public interface IAsanaOAuth
    {
        OAuthToken          OAuthToken { get; set; }

        event               RefreshTokenEventHandler RefreshTokenEvent;

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
        Task<IAsanaOAuth>   InitializeAsync(OAuthToken authorizationToken);
    }
}