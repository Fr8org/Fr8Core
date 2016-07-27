using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Utilities.Logging;
using Newtonsoft.Json.Linq;
using terminalAsana.Asana.Entities;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana.Services
{
    public class Tasks : IAsanaTasks
    {
        private IAsanaOAuthCommunicator _restClient;
        private IAsanaParameters _asanaParams;


        public Tasks(IAsanaOAuthCommunicator client, IAsanaParameters parameters)
        {
            _asanaParams = parameters;
            _restClient = client;
        }

        public Task<AsanaTask> CreateAsync(AsanaTask task)
        {
            throw new NotImplementedException();
        }

        public Task<AsanaTask> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<AsanaTask> UpdateAsync(AsanaTask task)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(AsanaTask task)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<AsanaTask>> GetAsync(AsanaTaskQuery query)
        {
            var baseUrl = _asanaParams.TasksUrl + "?";
            var url = baseUrl;
            url = query.Workspace != null ? url + $"workspace={query.Workspace}&": url;
            url = query.Assignee != null  ? url + $"assignee={query.Assignee}&" : url + $"assignee=me&";
            url = query.Project != null ? baseUrl + $"project={query.Project}&" : url;
            url = query.Tag != null ? baseUrl + $"tag={query.Tag}" : url;
            url = query.CompletedSince != null  ? url + $"completed_since={query.CompletedSince}&" : url;
            url = query.ModifiedSince != null ? url + $"modified_since={query.ModifiedSince}&" : url;
            

            var uri = new Uri(url);

            try
            {
                var response = await _restClient.GetAsync<JObject>(uri);
                var result = response.GetValue("data").ToObject<IEnumerable<AsanaTask>>();
                return result;
            }
            catch (Exception exp)
            {
                Logger.GetLogger().Error($"terminalAsana error = {exp.Message}");
                throw;
            }
        }

        public Task<IEnumerable<AsanaTask>> GetAllSubtasksAsync(string taskId)
        {
            throw new NotImplementedException();
        }

        public Task<AsanaTask> CreateSubTaskAsync(AsanaTask task)
        {
            throw new NotImplementedException();
        }

        public Task SetParentTaskAsync(AsanaTask task)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AsanaStory>> GetStoriesAsync(string taskId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AsanaProject>> GetProjectsAsync(string taskId)
        {
            throw new NotImplementedException();
        }

        public Task AddToProjectAsync(AsanaProjectInsertion query)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromProject(string taskId, string projectId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AsanaTag>> GetTags(string taskId)
        {
            throw new NotImplementedException();
        }

        public Task AddTag(string taskId, string tagId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveTag(string taskId, string tagId)
        {
            throw new NotImplementedException();
        }

        public Task<AsanaTask> AddFollowers(string taskId, IEnumerable<AsanaUser> followers)
        {
            throw new NotImplementedException();
        }

        public Task<AsanaTask> RemoveFollowers(string taskId, IEnumerable<AsanaUser> followers)
        {
            throw new NotImplementedException();
        }
    }
}