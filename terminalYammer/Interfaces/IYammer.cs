using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalYammer.Interfaces
{
    public interface IYammer
    {
        string CreateAuthUrl(string externalStateToken);
        Task<string> GetOAuthToken(string code);
        Task<string> GetUserId(string oauthToken);
        Task<List<KeyValueDTO>> GetGroupsList(string oauthToken);
        Task<bool> PostMessageToGroup(string oauthToken, string groupId, string message);
    }
}
