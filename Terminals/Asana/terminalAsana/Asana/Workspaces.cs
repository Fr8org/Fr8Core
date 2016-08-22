using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using terminalAsana.Asana.Entities;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana
{
    public class Workspaces : IAsanaWorkspaces
    {
        private IAsanaOAuthCommunicator _restClient;
        private IAsanaParameters _asanaParams;


        public Workspaces(IAsanaOAuthCommunicator client, IAsanaParameters asanaParams)
        {
            _restClient = client;
            _asanaParams = asanaParams;
        }

        public async Task<IEnumerable<AsanaWorkspace>> GetAsync()
        {
            var uri = new Uri(_asanaParams.WorkspacesUrl);
            try
            {
                var response = await _restClient.GetAsync<JObject>(uri).ConfigureAwait(false);
                var result = response.GetValue("data").ToObject<IEnumerable<AsanaWorkspace>>();
                return result;
            }
            catch (Exception exp)
            {
                Logger.GetLogger().Error($"terminalAsana error = {exp.Message}");
                throw;
            }
        }

        public Task<bool> UpdateWorkspaceAsync(AsanaWorkspace workspace)
        {
            throw new NotImplementedException();
        }

        public Task<AsanaWorkspace> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<AsanaWorkspace> UpdateAsync(AsanaWorkspace workspace)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> SearchAsync(WorkspaceSearchQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<AsanaUser> AddUserAsync(AsanaUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveUserAsync(AsanaUser user)
        {
            throw new NotImplementedException();
        }
    }
}