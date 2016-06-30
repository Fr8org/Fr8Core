using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using terminalAsana.Asana.Entities;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana.Services
{
    public class Tasks : IAsanaTasks
    {
        private IAsanaOAuthCommunicator _restClient;
        private IAsanaParameters _asanaParams;


        public Tasks(IAsanaParameters parameters, IAsanaOAuthCommunicator client)
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

        public Task<AsanaTask> Query(AsanaTaskQuery query)
        {
            throw new NotImplementedException();
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