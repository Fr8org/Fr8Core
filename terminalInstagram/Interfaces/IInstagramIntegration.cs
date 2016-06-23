using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace terminalInstagram.Interfaces
{
    public interface IInstagramIntegration
    {
        string CreateAuthUrl(string externalStateToken);
        Task<string> GetOAuthToken(string code);
        Task<UserData> GetUserInfo(string oauthToken);
    }

    public class UserInfo
    {
        [JsonProperty("username")]
        public string UserId { get; set; }
        [JsonProperty("full_name")]
        public string UserName { get; set; }
    }

    public class UserData
    {
        [JsonProperty("data")]
        public UserInfo User { get; set; }
    }
}
