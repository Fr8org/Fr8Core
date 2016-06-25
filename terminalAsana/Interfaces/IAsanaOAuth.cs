using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace terminalAsana.Interfaces
{
    public interface IAsanaOAuth
    {
        string      Token { get; set; }
        string      RefreshToken { get; set; }
        DateTime    ExpirationDate { get; set; }

        bool        ValidateToken();
        string      RefreshOAuthToken();
        void        RefreshTokenIfExpired();

        string        CreateAuthUrl(string state);
        Task<JObject> GetOAuthTokenData(string code);

        
    }
}