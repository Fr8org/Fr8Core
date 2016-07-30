using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Logging;
using Newtonsoft.Json.Linq;
using terminalAsana.Asana.Entities;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana
{
    public class Users : IAsanaUsers
    {
        private readonly IAsanaOAuthCommunicator _restfulClient;
        private readonly IAsanaParameters _asanaParams;

        public Users(IAsanaOAuthCommunicator client, IAsanaParameters asanaParams)
        {
            _restfulClient = client;
            _asanaParams = asanaParams;
        }

        public async Task<AsanaUser> MeAsync()
        {
            var uri = new Uri(_asanaParams.UsersMeUrl);

            try
            {
                var response = await _restfulClient.GetAsync<JObject>(uri);
                var result = response.GetValue("data").ToObject<AsanaUser>();
                return result;
            }
            catch (Exception exp)
            {
                Logger.GetLogger().Error($"terminalAsana error = {exp.Message}");
                throw;
            }
        }

        public async Task<AsanaUser> GetUserAsync(string userId)
        {
            var uri = new Uri(_asanaParams.UsersUrl.Replace("{user-id}", userId));

            try
            {
                var response = await _restfulClient.GetAsync<JObject>(uri);
                var result = response.GetValue("data").ToObject<AsanaUser>();
                return result;
            }
            catch (Exception exp)
            {
                Logger.GetLogger().Error($"terminalAsana error = {exp.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<AsanaUser>> GetUsersAsync(string  workspaceId)
        {
            var uri = new Uri(_asanaParams.UsersInWorkspaceUrl.Replace("{workspace-id}", workspaceId));

            try
            {
                var response = await _restfulClient.GetAsync<JObject>(uri);
                var result = response.GetValue("data").ToObject<IEnumerable<AsanaUser>>();
                return result;
            }
            catch (Exception exp)
            {
                Logger.GetLogger().Error($"terminalAsana error = {exp.Message}");
                throw;
            }
            
        }
    }
}