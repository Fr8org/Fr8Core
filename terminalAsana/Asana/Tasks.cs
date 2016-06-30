using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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

        public async Task<IEnumerable<AsanaTask>> Query(AsanaTaskQuery query)
        {
            var url = _asanaParams.TasksUrl + "?";
            url = query.Workspace != null ? url + $"workspace={query.Workspace}&": url;
            url = query.Assignee != null  ? url + $"assignee={query.Assignee}&" : url + $"assignee=me&";
            url = query.CompletedSince != null  ? url + $"completed_since={query.Workspace}&" : url;
            url = query.ModifiedSince != null ? url + $"modified_since={query.Workspace}&" : url;
            url = query.Project != null ? url + $"project={query.Workspace}&" : url;
            url = query.Tag != null ? url + $"tag={query.Workspace}" : url;

            var uri = new Uri(url);
            
            //_intergration.ApiCall()

            var response = Task.Run(() => _restClient.GetAsync<JObject>(uri)).Result;
            var result = response.GetValue("data").ToObject<IEnumerable<AsanaTask>>();

            return result;
            
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