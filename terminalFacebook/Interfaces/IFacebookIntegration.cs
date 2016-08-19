using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;
using terminalFacebook.Models;

namespace terminalFacebook.Interfaces
{
    public interface IFacebookIntegration
    {
        string CreateAuthUrl(string externalStateToken);
        Task<string> GetOAuthToken(string code);
        Task<UserInfo> GetUserInfo(string oauthToken);
        Task PostToTimeline(string oauthToken, string message);
        Task<FacebookPost> GetPostById(string oauthToken, string postId);
    }

    public class UserInfo
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }
        [JsonProperty("user")]
        public string UserName { get; set; }
    }
}
