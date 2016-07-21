using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Errors;
using Newtonsoft.Json.Linq;
using terminalSlack.Interfaces;

namespace terminalSlack.Services
{
    public class SlackIntegration : ISlackIntegration
    {
        private readonly IRestfulServiceClient _client;

        public SlackIntegration(IRestfulServiceClient client)
        {
            _client = client;
        }

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
            var jsonObj = await _client.GetAsync<JObject>(new Uri(url));
            return jsonObj.Value<string>("access_token");
        }

        public async Task<UserInfo> GetUserInfo(string oauthToken)
        {
            var url = PrepareTokenUrl("SlackAuthTestUrl", oauthToken);
            var response = await _client.GetAsync<JObject>(new Uri(url));
            if (!string.Equals(response.Value<string>("ok"), "true", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ApplicationException($"Failed to get Slack user info. Slack response code - {response.Value<string>("error")}");
            }
            return response.ToObject<UserInfo>();
        }

        private string PrepareTokenUrl(string urlKey, string oauthToken)
        {
            var template = CloudConfigurationManager.GetSetting(urlKey);
            var url = template.Replace("%TOKEN%", oauthToken);

            return url;
        }

        public async Task<List<KeyValueDTO>> GetChannelList(string oauthToken, bool includeArchived = false)
        {
            var url = $"{PrepareTokenUrl("SlackChannelsListUrl", oauthToken)}&exclude_archived={(includeArchived ? 0 : 1)}";

            var jsonObj = await _client.GetAsync<JObject>(new Uri(url));

            var channelsArray = jsonObj.Value<JArray>("channels");

            var result = new List<KeyValueDTO>();
            foreach (var channelObj in channelsArray)
            {
                var channelId = channelObj.Value<string>("id");
                var channelName = channelObj.Value<string>("name");

                result.Add(new KeyValueDTO()
                {
                    Key = channelName,
                    Value = channelId
                });
            }

            return result;

        }

        public async Task<List<KeyValueDTO>> GetUserList(string oauthToken)
        {
            var url = PrepareTokenUrl("SlackUserListUrl", oauthToken);
            var jsonObj = await _client.GetAsync<JObject>(new Uri(url));

            var usersArray = jsonObj.Value<JArray>("members");
            var result = new List<KeyValueDTO>();
            foreach (var userObj in usersArray)
            {
                if (userObj.Value<bool>("deleted"))
                    continue;
                var userId = userObj.Value<string>("id");
                var userName = userObj.Value<string>("name");

                result.Add(new KeyValueDTO()
                {
                    Key = userName,
                    Value = userId
                });
            }

            return result;
        }

        public async Task<List<KeyValueDTO>> GetAllChannelList(string oauthToken)
        {
            var channels = await GetChannelList(oauthToken);
            var users = await GetUserList(oauthToken);

            var result = new List<KeyValueDTO>();
            result.AddRange(channels);
            result.AddRange(users);
            return result;
        }



        public async Task<bool> PostMessageToChat(string oauthToken, string channelId, string message)
        {
            var url = CloudConfigurationManager.GetSetting("SlackChatPostMessageUrl");




            var content = new FormUrlEncodedContent(
                new[] {
                    new KeyValuePair<string, string>("token", oauthToken),
                    new KeyValuePair<string, string>("channel", channelId),
                    new KeyValuePair<string, string>("text", message)
                }
            );

            var responseJson = await _client.PostAsync<JObject>(new Uri(url), (HttpContent)content);
            bool isOk;
            try
            {
                isOk = responseJson.Value<bool>("ok");
            }
            catch (Exception ex)
            {
                return false;
            }

            if (!isOk)
            {
                string reason = responseJson.Value<string>("error");
                if (reason.IndexOf("token") > -1)
                {
                    throw new AuthorizationTokenExpiredOrInvalidException();
                }
            }
            return isOk;
        }
    }
}