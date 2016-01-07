using System;
using System.Threading.Tasks;

namespace Hub.Managers.APIManagers.Transmitters.Restful
{
    public interface IRestfulServiceClient
    {
        Uri BaseUri { get; set; }
        Task<TResponse> GetAsync<TResponse>(Uri requestUri, string CorrelationId = null);
        Task<string> PostAsync(Uri requestUri, string CorrelationId = null);
        Task<string> PostAsync<TContent>(Uri requestUri, TContent content, string CorrelationId = null);
        Task<TResponse> PostAsync<TContent, TResponse>(Uri requestUri, TContent content, string CorrelationId = null);
        Task<string> PutAsync<TContent>(Uri requestUri, TContent content, string CorrelationId = null);
        Task<TResponse> PutAsync<TContent, TResponse>(Uri requestUri, TContent content, string CorrelationId = null);
    }
}