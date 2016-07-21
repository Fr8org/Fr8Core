using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Hub.Managers.APIManagers.Transmitters.Terminal;

namespace HubTests.Services
{
    class TerminalTransmitterMock : ITerminalTransmitter
    {
        public Uri BaseUri { get; set; }

        public Func<string, IEnumerable<KeyValuePair<string, string>>, Fr8DataDTO, object> CallActivityBody { get; set; }
        
        public Task<TResponse> CallActivityAsync<TResponse>(string actionType,
            IEnumerable<KeyValuePair<string, string>> parameters, Fr8DataDTO dataDTO, string correlationId)
        {
            return Task.FromResult((TResponse) CallActivityBody(actionType, parameters, dataDTO));
        }

        public Task<Stream> DownloadAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> GetAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
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

        public Task<TResponse> PostAsync<TContent, TResponse>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> PutAsync<TContent, TResponse>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> PutAsync<TContent>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }
        
        public Task<string> PutAsync(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public void AddRequestSignature(IRequestSignature signature)
        {
        }

        public Task<TResponse> PutAsync<TResponse>(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> PostAsync<TResponse>(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> PostAsync(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }
    }
}
