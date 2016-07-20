using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace terminalSlack.Interfaces
{
    public interface ISlackIntegration
    {
        string CreateAuthUrl(string externalStateToken);
        Task<string> GetOAuthToken(string code);
        Task<UserInfo> GetUserInfo(string oauthToken);
        Task<List<KeyValueDTO>> GetChannelList(string oauthToken, bool includeArchived = false);
        Task<List<KeyValueDTO>> GetUserList(string oauthToken);
        Task<List<KeyValueDTO>> GetAllChannelList(string oauthToken);
        Task<bool> PostMessageToChat(string oauthToken, string channelId, string message);
    }

    public class UserInfo
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }
        [JsonProperty("user")]
        public string UserName { get; set; }
        [JsonProperty("team_id")]
        public string TeamId { get; set; }
        [JsonProperty("team")]
        public string TeamName { get; set; }
    }
}
