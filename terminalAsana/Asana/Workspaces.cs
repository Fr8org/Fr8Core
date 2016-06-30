using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Interfaces;
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

        public IEnumerable<AsanaWorkspace> GetAll()
        {
            var uri = new Uri(_asanaParams.WorkspacesUrl);
            //_intergration.ApiCall()
          
            var response = Task.Run(() => _restClient.GetAsync<JObject>(uri)).Result; 
            var result = response.GetValue("data").ToObject<IEnumerable<AsanaWorkspace>>();

            return result;
        }

        public bool UpdateWorkspace(AsanaWorkspace workspace)
        {
            throw new NotImplementedException();
        }

        public AsanaWorkspace Get(int id)
        {
            throw new NotImplementedException();
        }


        public AsanaWorkspace Update(AsanaWorkspace workspace)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Search(WorkspaceSearchQuery query)
        {
            throw new NotImplementedException();
        }

        public AsanaUser AddUser(AsanaUser user)
        {
            throw new NotImplementedException();
        }

        public bool RemoveUser(AsanaUser user)
        {
            throw new NotImplementedException();
        }
    }
}