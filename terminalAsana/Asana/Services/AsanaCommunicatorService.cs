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

        /// <summary>
        /// Add OAuth access_token to headers
        /// </summary>
        /// <param name="currentHeader"></param>
        /// <returns></returns>
        public async Task<Dictionary<string,string>> PrepareHeader(Dictionary<string,string> currentHeader)
        {
            var token = await OAuthService.RefreshTokenIfExpiredAsync();
            var headers = new Dictionary<string, string>()
            {
                {"Authorization", $"Bearer {token.AccessToken}"},
            };
            
            var combinedHeaders = currentHeader?.Concat(headers).ToDictionary(k => k.Key, v => v.Value) ?? headers;
            return combinedHeaders;
        }

        public async Task<TResponse> GetAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            var header = await PrepareHeader(headers);             
            var response = await _restfulClient.GetAsync<TResponse>(requestUri, CorrelationId, header);
            return response;
        }

        public async Task<TResponse> PostAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            var header = await PrepareHeader(headers);
            var response = await _restfulClient.PostAsync<TResponse>(requestUri, CorrelationId, header);
            return response;
        }


        public async Task<TResponse> PostAsync<TResponse>(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            var header = await PrepareHeader(headers);
            var response = await _restfulClient.PostAsync<TResponse>(requestUri, content, CorrelationId, header);
            return response;
        }

    }
}