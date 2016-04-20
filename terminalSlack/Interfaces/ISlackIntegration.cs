using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;

namespace terminalSlack.Interfaces
{
    public interface ISlackIntegration
    {
        string CreateAuthUrl(string externalStateToken);
        Task<string> GetOAuthToken(string code);
        Task<string> GetUserId(string oauthToken);
        Task<string> GetUserName(string oauthToken);
        Task<List<FieldDTO>> GetChannelList(string oauthToken, bool includeArchived = false);
        Task<List<FieldDTO>> GetUserList(string oauthToken);
        Task<List<FieldDTO>> GetAllChannelList(string oauthToken);
        Task<bool> PostMessageToChat(string oauthToken, string channelId, string message);
    }
}
