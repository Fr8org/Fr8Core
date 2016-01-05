using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hub.Managers.APIManagers.Transmitters.Restful
{
    public interface IRestfulServiceClient
    {
        Uri BaseUri { get; set; }
        Task<TResponse> GetAsync<TResponse>(Uri requestUri);
        Task<string> PostAsync(Uri requestUri);
        Task<string> PostAsync<TContent>(Uri requestUri, TContent content);
        Task<TResponse> PostAsync<TContent, TResponse>(Uri requestUri, TContent content);
        Task<TResponse> PostAsync<TResponse>(Uri requestUri, HttpContent content);
        Task<string> PutAsync<TContent>(Uri requestUri, TContent content);
        Task<TResponse> PutAsync<TContent, TResponse>(Uri requestUri, TContent content);
    }
}