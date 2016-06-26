using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
        OAuthToken          RefreshOAuthToken();
        OAuthToken          RefreshOAuthToken(OAuthToken token);
        OAuthToken          RefreshTokenIfExpired();
        OAuthToken          RefreshTokenIfExpired(OAuthToken token);

        DateTime            CalculateExpirationTime(int secondsToExpiration);
        string              CreateAuthUrl(string state);
        Task<JObject>       GetOAuthTokenData(string code);

        bool                IsIntialized { get; }
        IAsanaOAuth         Initialize(AuthorizationToken authorizationToken);
    }
}