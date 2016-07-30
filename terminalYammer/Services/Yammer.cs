using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Errors;
using Newtonsoft.Json.Linq;
using terminalYammer.Interfaces;
using terminalYammer.Model;

namespace terminalYammer.Services
{
    public class Yammer : IYammer
    {
        private readonly IRestfulServiceClient _client;
        public Yammer(IRestfulServiceClient restfulServiceClient)
        {
            _client = restfulServiceClient;
        }
        /// <summary>
        /// Build external Yammer OAuth url.
        /// </summary>
        public string CreateAuthUrl(string externalStateToken)
        {
            var url = CloudConfigurationManager.GetSetting("YammerOAuthUrl");
            url = url.Replace("%STATE%", externalStateToken);

            return url;
        }

        public async Task<string> GetOAuthToken(string code)
        {
            var template = CloudConfigurationManager.GetSetting("YammerOAuthAccessUrl");
            var url = template.Replace("%CODE%", code);
            var authEnvelope = await _client.GetAsync<YammerAccessToken>(new Uri(url));
            return authEnvelope.TokenResponse.Token;    
        }

        private string PrepareTokenUrl(string urlKey, string oauthToken)
        {
            var template = CloudConfigurationManager.GetSetting(urlKey);
            var url = template.Replace("%TOKEN%", oauthToken);

            return url;
        }

        private Dictionary<string, string> GetAuthenticationHeader(string oauthToken)
        {
            return new Dictionary<string, string>
            {
                { System.Net.HttpRequestHeader.Authorization.ToString(), string.Format("Bearer {0}", oauthToken) }
            };
        }

        public async Task<string> GetUserId(string oauthToken)
        {
            var url = PrepareTokenUrl("YammerOAuthCurrentUserUrl", oauthToken);
            var jsonObj = await _client.GetAsync<JObject>(new Uri(url), null, GetAuthenticationHeader(oauthToken));
            return jsonObj.Value<string>("email");
        }

        public async Task<List<KeyValueDTO>> GetGroupsList(string oauthToken)
        {
            var url = PrepareTokenUrl("YammerGroupListUrl", oauthToken);

            var groupsDTO = await _client.GetAsync<List<YammerGroup>>(new Uri(url), null, GetAuthenticationHeader(oauthToken));
            var result = new List<KeyValueDTO>();
            foreach (var group in groupsDTO)
            {
                result.Add(new KeyValueDTO()
                {
                    Key = group.Name,
                    Value = group.GroupID
                });
            }

            return result;
            
        }

        public async Task<bool> PostMessageToGroup(string oauthToken, string groupId, string message)
        {
            var url = CloudConfigurationManager.GetSetting("YammerPostMessageUrl");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + oauthToken);

            var content = new FormUrlEncodedContent(
                new[] { 
                    new KeyValuePair<string, string>("group_id", groupId),
                    new KeyValuePair<string, string>("body", message)
                }
            );

            using (var response = await httpClient.PostAsync(url, content))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return true;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AuthorizationTokenExpiredOrInvalidException();
                }
                return false;
            }
        }
    }
}