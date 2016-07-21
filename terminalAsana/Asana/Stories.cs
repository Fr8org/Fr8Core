using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Utilities.Logging;
using Newtonsoft.Json.Linq;
using terminalAsana.Asana.Entities;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana
{
    public class Stories : IAsanaStories
    {
        private IAsanaOAuthCommunicator _restClient;
        private IAsanaParameters _asanaParams;

        public Stories(IAsanaOAuthCommunicator client, IAsanaParameters asanaParams)
        {
            _restClient = client;
            _asanaParams = asanaParams;
        }

        public Task<IEnumerable<AsanaStory>> GetTaskStoriesAsync(string taskId)
        {
            throw new NotImplementedException();
        }

        public Task<AsanaStory> GetAsync(string storyId)
        {
            throw new NotImplementedException();
        }

        public async Task<AsanaStory> PostCommentAsync(string taskId, string commentText)
        {
            var uri = new Uri(_asanaParams.StoriesUrl.Replace("{task-id}",taskId));

            try
            {
                var contentDic = new Dictionary<string, string>()
                {
                    {"text",commentText}
                };
                var content = new FormUrlEncodedContent(contentDic);

                var response = await _restClient.PostAsync<JObject>(uri,content).ConfigureAwait(false);
                var result = response.GetValue("data").ToObject<AsanaStory>();
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