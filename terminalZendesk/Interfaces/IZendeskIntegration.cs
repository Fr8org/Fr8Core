using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace terminalZendesk.Interfaces
{
    public interface IZendeskIntegration
    {
        string CreateAuthUrl(string externalStateToken);
        Task<string> GetOAuthToken(string code);
        Task<UserInfo> GetUserInfo(string oauthToken);

        Task CreateTicket(string oauthToken, string subject, string comment, string reqEmail, string reqName);
    }

    public class UserInfo
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }
        [JsonProperty("user")]
        public string UserName { get; set; }
    }
}
