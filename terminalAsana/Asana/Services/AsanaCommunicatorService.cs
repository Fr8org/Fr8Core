using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Interfaces;
using Newtonsoft.Json.Linq;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana.Services
{
    
    public class AsanaCommunicatorService : IAsanaOAuthCommunicator
    {
        public IAsanaOAuth OAuthService { get; set;}

        private IRestfulServiceClient _restfulClient;

        public AsanaCommunicatorService(IAsanaOAuth oauth, IRestfulServiceClient client)
        {
            OAuthService = oauth;
            _restfulClient = client;
        }

        private async Task<Dictionary<string,string>> PrepareHeader()
        {
            var token = await OAuthService.RefreshTokenIfExpiredAsync();
            var headers = new Dictionary<string, string>()
            {
                {"Authorization", $"Bearer {token.AccessToken}"},
            };
            return headers;
        }
        
        public Task<Stream> DownloadAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public async Task<TResponse> GetAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            var header = await PrepareHeader();
            var combinedHeaders = headers == null ? header : headers.Concat(header); 

            var response = await _restfulClient.GetAsync<TResponse>(requestUri, CorrelationId, combinedHeaders.ToDictionary( k=>k.Key, v=>v.Value));

            return response;
            //throw new NotImplementedException();
        }

        public Task<string> GetAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> PostAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> PostAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> PostAsync<TContent>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> PostAsync<TContent, TResponse>(Uri requestUri, TContent content, string CorrelationId = null,
            Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> PutAsync<TContent, TResponse>(Uri requestUri, TContent content, string CorrelationId = null,
            Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> PutAsync<TContent>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> PostAsync(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> PostAsync<TResponse>(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> PutAsync<TResponse>(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> PutAsync(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }
    }
}