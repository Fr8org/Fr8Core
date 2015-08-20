using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.Managers.APIManagers.Packagers.Json;
using Newtonsoft.Json;

namespace Core.Managers.APIManagers.Transmitters.Restful
{
    public class RestfulServiceClient : IRestfulServiceClient
    {
        private readonly HttpClient _innerClient;

        public RestfulServiceClient()
        {
            _innerClient = new HttpClient();
        }

        private async Task<HttpResponseMessage> SendInternalAsync(HttpRequestMessage request)
        {
            var response = await _innerClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = ExtractErrorMessage(responseContent);
                throw new RestfulServiceException(errorMessage, ex);
            }
            return response;
        }

        protected virtual string ExtractErrorMessage(string responseContent)
        {
            return responseContent;
        }

        private async Task<HttpResponseMessage> GetInternalAsync(Uri requestUri)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                return await SendInternalAsync(request);
            }
        }

        private async Task<HttpResponseMessage> PostInternalAsync<TContent>(Uri requestUri, TContent content)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri) {Content = JsonContent.FromObject(content)})
            {
                return await SendInternalAsync(request);
            }
        }

        private async Task<HttpResponseMessage> PutInternalAsync<TContent>(Uri requestUri, TContent content)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, requestUri) { Content = JsonContent.FromObject(content) })
            {
                return await SendInternalAsync(request);
            }
        }

        public Uri BaseUri
        {
            get { return _innerClient.BaseAddress; }
            set { _innerClient.BaseAddress = value; }
        }

        public async Task<TResponse> GetAsync<TResponse>(Uri requestUri)
        {
            using (var response = await GetInternalAsync(requestUri))
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TResponse>(responseContent);
            }
        }

        public async Task<string> PostAsync<TContent>(Uri requestUri, TContent content)
        {
            using (var response = await PostInternalAsync(requestUri, content))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<TResponse> PostAsync<TContent, TResponse>(Uri requestUri, TContent content)
        {
            using (var response = await PostInternalAsync(requestUri, content))
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TResponse>(responseContent);
            }
        }

        public async Task<TResponse> PutAsync<TContent, TResponse>(Uri requestUri, TContent content)
        {
            using (var response = await PutInternalAsync(requestUri, content))
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TResponse>(responseContent);
            }
        }

        public async Task<string> PutAsync<TContent>(Uri requestUri, TContent content)
        {
            using (var response = await PutInternalAsync(requestUri, content))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
