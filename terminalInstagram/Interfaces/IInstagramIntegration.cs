using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;
using terminalInstagram.Models;

namespace terminalInstagram.Interfaces
{
    public interface IInstagramIntegration
    {
        string CreateAuthUrl(string externalStateToken);
        Task<string> GetOAuthToken(string code);
        Task<UserData> GetUserInfo(string oauthToken);
        Task<InstagramPost> GetPostById(string oauthToken, string mediaId);
    }

    public class UserInfo
    {
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("full_name")]
        public string UserFullName { get; set; }
        [JsonProperty("id")]
        public string UserId { get; set; }
    }

    public class UserData
    {
        [JsonProperty("data")]
        public UserInfo User { get; set; }
    }
}
