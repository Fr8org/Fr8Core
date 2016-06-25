using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana
{
    public class AsanaOAuthService: IAsanaOAuth
    {
        private readonly IRestfulServiceClient _restfulClient;

        public AsanaOAuthService(IRestfulServiceClient client)
        {
            _restfulClient = client;
        }

        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpirationDate { get; set; }


        public bool ValidateToken()
        {
            throw new NotImplementedException();
        }

        public string RefreshOAuthToken()
        {
            throw new NotImplementedException();
        }

        public void RefreshTokenIfExpired()
        {
            throw new NotImplementedException();
        }

        public string CreateAuthUrl(string state)
        {
            var resultUrl = CloudConfigurationManager.GetSetting("AsanaOAuthCodeUrl");
            resultUrl = resultUrl.Replace("%STATE%", state);
            return resultUrl;
        }

        public async Task<JObject> GetOAuthTokenData(string code)
        {
            var url = CloudConfigurationManager.GetSetting("AsanaOAuthTokenUrl");
            var contentDic = new Dictionary<string, string>()
            {
                {"grant_type", "authorization_code" },
                {"client_id", CloudConfigurationManager.GetSetting("AsanaClientId") },
                {"client_secret", CloudConfigurationManager.GetSetting("AsanaClientSecret") },
                {"redirect_uri", HttpUtility.UrlDecode(CloudConfigurationManager.GetSetting("AsanaOriginalRedirectUrl")) },
                {"code",HttpUtility.UrlDecode(code)}
            };
            
            var content = new FormUrlEncodedContent(contentDic);

            var jsonObj = await _restfulClient.PostAsync<JObject>(new Uri(url), content);
            return jsonObj; //.Value<string>("access_token");
        }

       
    }
}