using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Data.Interfaces.DataTransferObjects;
using pluginSlack.Interfaces;

namespace pluginSlack.Services
{
    public class SlackIntegration : ISlackIntegration
    {
        /// <summary>
        /// Build external Slack OAuth url.
        /// </summary>
        public string CreateAuthUrl(string externalStateToken)
        {
            var template = ConfigurationManager.AppSettings["SlackOAuthUrl"];
            var url = template.Replace("%STATE%", externalStateToken);

            return url;
        }

        public async Task<string> GetOAuthToken(string code)
        {
            var template = ConfigurationManager.AppSettings["SlackOAuthAccessUrl"];
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
            var template = ConfigurationManager.AppSettings[urlKey];
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
    }
}