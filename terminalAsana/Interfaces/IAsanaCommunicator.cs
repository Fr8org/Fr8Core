using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;

namespace terminalAsana.Interfaces
{
    public interface IAsanaOAuthCommunicator 
    {
        IAsanaOAuth OAuthService { get; set; }
        Task<Dictionary<string, string>> PrepareHeader(Dictionary<string, string> existingHeaders);

        //what was left from IRestClientService
        Task<TResponse> GetAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null);
        Task<TResponse> PostAsync<TResponse>(Uri requestUri, string CorrelationId = null,Dictionary<string, string> headers = null);
        Task<TResponse> PostAsync<TResponse>(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null);
    }
}
