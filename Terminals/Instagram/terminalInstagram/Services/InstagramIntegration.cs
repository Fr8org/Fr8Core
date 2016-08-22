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
using System.Collections.Specialized;
using terminalInstagram.Models;

namespace terminalInstagram.Services
{
    public class InstagramIntegration : IInstagramIntegration
    {
        private readonly IRestfulServiceClient _client;
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
            var link = "https://api.instagram.com/oauth/authorize/?client_id=" + clientId + "&redirect_uri=" + redirectUri.Replace("&", "%26") + "&response_type=code&state=" + externalStateToken;
            return link; 
        }

        public async Task<string> GetOAuthToken(string code)
        {
            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("client_id", clientId));
            parameters.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
            parameters.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            parameters.Add(new KeyValuePair<string, string>("redirect_uri",redirectUri));
            parameters.Add(new KeyValuePair<string, string>("code", code));
            var formContent = new FormUrlEncodedContent(parameters);

            var url = new Uri("https://api.instagram.com/oauth/access_token");
            var jsonObj = await _client.PostAsync<JObject>(url, formContent);
            return jsonObj.Value<string>("access_token");
        }

        private string PrepareTokenUrl(string urlKey, string oauthToken)
        {
            var template = CloudConfigurationManager.GetSetting(urlKey);
            var url = template.Replace("%TOKEN%", oauthToken);

            return url;
        }
        public async Task<UserData> GetUserInfo(string oauthToken)
        {
            var response = await _client.GetAsync<JObject>(new Uri("https://api.instagram.com/v1/users/self/?access_token=" + oauthToken));
            return response.ToObject<UserData>();
        }

        public async Task<InstagramPost> GetPostById(string mediaId, string oauthToken)
        {
            var response = await _client.GetAsync<JObject>(new Uri("https://api.instagram.com/v1/media/" + mediaId + "?access_token=" + oauthToken));
            return response.ToObject<InstagramPost>();
        }
    }
}