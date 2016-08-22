using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Utilities.Logging;
using terminalAsana.Asana.Entities;
using terminalAsana.Interfaces;
using Newtonsoft.Json.Linq;

namespace terminalAsana.Asana
{
    public class Projects:IAsanaProjects
    {
        private IAsanaOAuthCommunicator _restClient;
        private IAsanaParameters _asanaParams;

        public Projects(IAsanaOAuthCommunicator client, IAsanaParameters asanaParams)
        {
            _restClient = client;
            _asanaParams = asanaParams;
        }

        public Task<AsanaProject> CreateProject(AsanaProject project)
        {
            throw new NotImplementedException();
        }

        public Task<AsanaProject> Get(string projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<AsanaProject>> Get(AsanaProjectQuery query)
        {
            var uri = new Uri(_asanaParams.ProjectsUrl);
            try
            {
                var response = await _restClient.GetAsync<JObject>(uri);
                var result = response.GetValue("data").ToObject<IEnumerable<AsanaProject>>();
                return result;
            }
            catch (Exception exp)
            {
                Logger.GetLogger().Error($"terminalAsana error = {exp.Message}");
                throw;
            }


        }

        public Task<AsanaProject> Update(AsanaProject project)
        {
            throw new NotImplementedException();
        }

        public Task Delete(string projectId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AsanaTask>> GetTasks(string projectId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AsanaSection>> GetSections(string projectId)
        {
            throw new NotImplementedException();
        }
    }
}