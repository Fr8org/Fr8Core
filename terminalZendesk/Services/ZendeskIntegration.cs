using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Errors;
using Newtonsoft.Json.Linq;
using terminalZendesk.Interfaces;

namespace terminalZendesk.Services
{
    public class ZendeskIntegration : IZendeskIntegration
    {
        private readonly IRestfulServiceClient _client;

        public ZendeskIntegration(IRestfulServiceClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Build external Zendesk OAuth url.
        /// </summary>
        public string CreateAuthUrl(string externalStateToken)
        {
            var template = CloudConfigurationManager.GetSetting("ZendeskOAuthUrl");
            var url = template.Replace("%STATE%", externalStateToken);

            return url;
        }

        public async Task<string> GetOAuthToken(string code)
        {
            var parameters = new
            {
                grant_type = "authorization_code",
                code = code,
                client_id = CloudConfigurationManager.GetSetting("ZendeskId"),
                client_secret = CloudConfigurationManager.GetSetting("ZendeskSecret"),
                redirect_uri = CloudConfigurationManager.GetSetting("HubOAuthRedirectUri"),
                scope = "read"
            };
            
            var template = CloudConfigurationManager.GetSetting("ZendeskOAuthAccessUrl");
            var jsonObj = JObject.Parse(await _client.PostAsync(new Uri(template), parameters));
            return jsonObj.Value<string>("access_token");
        }

        public async Task<UserInfo> GetUserInfo(string oauthToken)
        {
            return new UserInfo
            {
                UserId = "test",
                UserName = "test",
                TeamName = "test"
            };
        }

    }
}