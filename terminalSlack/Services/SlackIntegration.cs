using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using terminalSlack.Interfaces;
using Utilities.Configuration.Azure;

namespace terminalSlack.Services
{
    public class SlackIntegration : ISlackIntegration
    {
        /// <summary>
        /// Build external Slack OAuth url.
        /// </summary>
        public string CreateAuthUrl(string externalStateToken)
        {
            var template = CloudConfigurationManager.GetSetting("SlackOAuthUrl");
            var url = template.Replace("%STATE%", externalStateToken);

            return url;
        }

        public async Task<string> GetOAuthToken(string code)
        {
            var template = CloudConfigurationManager.GetSetting("SlackOAuthAccessUrl");
            var url = template.Replace("%CODE%", code);

            var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(url))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonObj = JsonConvert.DeserializeObject<JObject>(responseString);

                return jsonObj.Value<string>("access_token");
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
            var url = PrepareTokenUrl("SlackAuthTestUrl", oauthToken);

            var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(url))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonObj = JsonConvert.DeserializeObject<JObject>(responseString);

                return jsonObj.Value<string>("user_id");
            }
        }

        public async Task<List<FieldDTO>> GetChannelList(string oauthToken)
        {
            var url = PrepareTokenUrl("SlackChannelsListUrl", oauthToken);

            var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(url))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonObj = JsonConvert.DeserializeObject<JObject>(responseString);

                var channelsArray = jsonObj.Value<JArray>("channels");

                var result = new List<FieldDTO>();
                foreach (var channelObj in channelsArray)
                {
                    var channelId = channelObj.Value<string>("id");
                    var channelName = channelObj.Value<string>("name");

                    result.Add(new FieldDTO()
                    {
                        Key = channelName,
                        Value = channelId
                    });
                }

                return result;
            }
        }

        public async Task<List<FieldDTO>> GetUserList(string oauthToken)
        {
            var url = PrepareTokenUrl("SlackUserListUrl", oauthToken);

            var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(url))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonObj = JsonConvert.DeserializeObject<JObject>(responseString);

                var usersArray = jsonObj.Value<JArray>("members");

                var result = new List<FieldDTO>();
                foreach (var userObj in usersArray)
                {
                    if (userObj.Value<bool>("deleted"))
                        continue;
                    var userId = userObj.Value<string>("id");
                    var userName = userObj.Value<string>("name");

                    result.Add(new FieldDTO()
                    {
                        Key = userName,
                        Value = userId
                    });
                }

                return result;
            }
        }

        public async Task<List<FieldDTO>> GetAllChannelList(string oauthToken)
        {
            var channels = await GetChannelList(oauthToken);
            var users = await GetUserList(oauthToken);

            var result = new List<FieldDTO>();
            result.AddRange(channels);
            result.AddRange(users);
            return result;
        }



        public async Task<bool> PostMessageToChat(string oauthToken, string channelId, string message)
        {
            var url = CloudConfigurationManager.GetSetting("SlackChatPostMessageUrl");

            var httpClient = new HttpClient();
            var content = new FormUrlEncodedContent(
                new[] { 
                    new KeyValuePair<string, string>("token", oauthToken),
                    new KeyValuePair<string, string>("channel", channelId),
                    new KeyValuePair<string, string>("text", message)
                }
            );

            using (var response = await httpClient.PostAsync(url, content))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JsonConvert.DeserializeObject<JObject>(responseString);

                try
                {
                    return responseJson.Value<bool>("ok");
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}