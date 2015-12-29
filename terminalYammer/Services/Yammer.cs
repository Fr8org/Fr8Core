using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using terminalYammer.Interfaces;
using Utilities.Configuration.Azure;
using terminalYammer.Model;

namespace terminalYammer.Services
{
    public class Yammer : IYammer
    {
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

            var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(url))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var authEnvelope = JsonConvert.DeserializeObject<YammerAccessToken>(responseString);

                return authEnvelope.TokenResponse.Token;    
            }
        }

        private string PrepareTokenUrl(string urlKey, string oauthToken)
        {
            var template = CloudConfigurationManager.GetSetting(urlKey);
            var url = template.Replace("%TOKEN%", oauthToken);

            return url;
        }

        public async Task<string> GetUserId(string oauthToken)
        {
            var url = PrepareTokenUrl("YammmerOAuthTestUrl", oauthToken);

            var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(url))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonObj = JsonConvert.DeserializeObject<JObject>(responseString);

                return jsonObj.Value<string>("user_id");
            }
        }

        public async Task<List<FieldDTO>> GetGroupsList(string oauthToken)
        {
            var url = PrepareTokenUrl("YammerGroupListUrl", oauthToken);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + oauthToken);
            using (var response = await httpClient.GetAsync(url))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var groupsDTO = JsonConvert.DeserializeObject<List<YammerGroup>>(responseString);

                var result = new List<FieldDTO>();
                foreach (var group in groupsDTO)
                {
                    result.Add(new FieldDTO()
                    {
                        Key = group.Name,
                        Value = group.GroupID
                    });
                }

                return result;
            }
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
                return false;
            }
        }
    }
}