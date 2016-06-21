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

namespace terminalInstagram.Services
{
    public class InstagramIntegration : IInstagramIntegration
    {
        private readonly IRestfulServiceClient _client;
        private string authCallbackURLDomain = CloudConfigurationManager.GetSetting("InstagramAuthCallbackURLDomain");
        private string clientId = CloudConfigurationManager.GetSetting("InstagramClientId");
        private string clientSecret = CloudConfigurationManager.GetSetting("InstagramClientSecret");
        private string redirectUri = CloudConfigurationManager.GetSetting("InstagramRedirectUri");
        public InstagramIntegration(IRestfulServiceClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Build external Instagram OAuth url.
        /// </summary>
        public string CreateAuthUrl(string externalStateToken)
        {
            var link = "https://api.instagram.com/oauth/authorize/?client_id=" + clientId + "&redirect_uri=" + redirectUri + "&response_type=code&state=" + externalStateToken;
            return link; 
        }

        public async Task<string> GetOAuthToken(string code)
        {
            var client = new HttpClient { BaseAddress = new Uri("https://api.instagram.com/oauth/access_token") };
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(client.BaseAddress, "access_token"));
            var myParameters = string.Format("client_id={0}&client_secret={1}&grant_type={2}&redirect_uri={3}&code={4}",
                                              clientId,
                                              clientSecret, 
                                              "authorization_code",
                                              redirectUri, 
                                              code);

            request.Content = new StringContent(myParameters);
            return client.ExecuteAsync<OAuthResponse>(request);
            var url = new Uri("https://api.instagram.com/oauth/access_token");
            var jsonObj = await _client.PostAsync<object, JObject>(url, new {client_id = clientId,
                                                                             client_secret = clientSecret,
                                                                             grant_type = "authorization_code",
                                                                             redirect_url = redirectUri,
                                                                             code = code});
            return jsonObj.Value<string>("access_token");
        }

        private string PrepareTokenUrl(string urlKey, string oauthToken)
        {
            var template = CloudConfigurationManager.GetSetting(urlKey);
            var url = template.Replace("%TOKEN%", oauthToken);

            return url;
        }
    }
}