using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Errors;
using Newtonsoft.Json.Linq;
using terminalInstagram.Interfaces;
using InstaSharp;

namespace terminalInstagram.Services
{
    public class InstagramIntegration : IInstagramIntegration
    {

        public InstagramIntegration()
        {
        }

        /// <summary>
        /// Build external Slack OAuth url.
        /// </summary>
        public string CreateAuthUrl(string externalStateToken)
        {
            var config = getInstagramConfig();
            return config.OAuthUri;
        }

        public async Task<string> GetOAuthToken(string code)
        {
            var auth = new OAuth(getInstagramConfig());
            var oauthResponse = await auth.RequestToken(code);
            return oauthResponse.AccessToken;
        }

        public InstagramConfig getInstagramConfig()
        {
            var authCallbackURLDomain = CloudConfigurationManager.GetSetting("InstagramAuthCallbackURLDomain");
            var clientId = CloudConfigurationManager.GetSetting("InstagramClientId");
            var clientSecret = CloudConfigurationManager.GetSetting("InstagramClientSecret");
            var redirectUri = CloudConfigurationManager.GetSetting("InstagramRedirectUri");
            var config = new InstagramConfig(clientId, clientSecret, redirectUri, "");
            return config;
        }

        private string PrepareTokenUrl(string urlKey, string oauthToken)
        {
            var template = CloudConfigurationManager.GetSetting(urlKey);
            var url = template.Replace("%TOKEN%", oauthToken);

            return url;
        }
    }
}