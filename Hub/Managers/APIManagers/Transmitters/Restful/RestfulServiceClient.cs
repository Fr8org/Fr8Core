using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Hub.Managers.APIManagers.Packagers.Json;
using Utilities.Logging;

namespace Hub.Managers.APIManagers.Transmitters.Restful
{
    public class RestfulServiceClient : IRestfulServiceClient
    {
        private static readonly ILog Log = Logger.GetLogger();
        class FormatterLogger : IFormatterLogger
        {
            public void LogError(string message, Exception ex)
            {
                Log.Error(message, ex);
            }

            public void LogError(string errorPath, string errorMessage)
            {
                Log.Error(string.Format("{0}: {1}", errorPath, errorMessage));
            }
        }

        private readonly HttpClient _innerClient;
        private readonly MediaTypeFormatter _formatter;
        private readonly FormatterLogger _formatterLogger;

        /// <summary>
        /// Creates an instance with JSON formatter for requests and responses
        /// </summary>
        public RestfulServiceClient()
            : this(new JsonMediaTypeFormatter())
        {
        }

        /// <summary>
        /// Creates an instance with specified formatter for requests and responses
        /// </summary>
        public RestfulServiceClient(MediaTypeFormatter formatter)
        {
            _innerClient = new HttpClient();
            _formatter = formatter;
            _formatterLogger = new FormatterLogger();
            _innerClient.Timeout = new TimeSpan(0, 10, 0); //10 minutes
        }

        private async Task<HttpResponseMessage> SendInternalAsync(HttpRequestMessage request)
        {
            HttpResponseMessage response;
            string responseContent = "";
            int statusCode = -1;

            try
            {
                response = await _innerClient.SendAsync(request);
                responseContent = await response.Content.ReadAsStringAsync();
                statusCode = (int)response.StatusCode;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                string errorMessage = String.Format("An error has ocurred while sending a {0} request to {1}. Response message:\r\n",
                    request.RequestUri,
                    request.Method.Method,
                    ExtractErrorMessage(responseContent));
                throw new RestfulServiceException(statusCode, errorMessage, ex);
            }
            catch (TaskCanceledException)
            {
                //Timeout
                throw new TimeoutException(
                    String.Format("Timeout while making HTTP request.  \r\nURL: {0},   \r\nMethod: {1}",
                    request.RequestUri,
                    request.Method.Method));
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("An error has ocurred while sending a {0} request to {1}.",
                    request.RequestUri,
                    request.Method.Method);
                throw new ApplicationException(errorMessage);
            }
            return response;
        }

        //private async Task<HttpResponseMessage> SendInternalAsync(HttpRequestMessage request)
        //{
        //    var response = await _innerClient.SendAsync(request);
        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    try
        //    {
        //        response.EnsureSuccessStatusCode();
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        var errorMessage = ExtractErrorMessage(responseContent);
        //        throw new RestfulServiceException(errorMessage, ex);
        //    }
        //    return response;
        //}



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

        private async Task<HttpResponseMessage> PostInternalAsync(Uri requestUri)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                return await SendInternalAsync(request);
            }
        }

        private async Task<HttpResponseMessage> PostInternalAsync<TContent>(Uri requestUri, TContent content)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = new ObjectContent(typeof(TContent), content, _formatter) })
            {
                return await SendInternalAsync(request);
            }
        }

        private async Task<HttpResponseMessage> PutInternalAsync<TContent>(Uri requestUri, TContent content)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, requestUri) { Content = new ObjectContent(typeof(TContent), content, _formatter) })
            {
                return await SendInternalAsync(request);
            }
        }

        private async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            var responseStream = await response.Content.ReadAsStreamAsync();
            var responseObject = await _formatter.ReadFromStreamAsync(
                typeof(T),
                responseStream,
                response.Content,
                _formatterLogger);
            return (T)responseObject;
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
                return await DeserializeResponseAsync<TResponse>(response);
            }
        }

        public async Task<string> PostAsync(Uri requestUri)
        {
            using (var response = await PostInternalAsync(requestUri))
            {
                return await response.Content.ReadAsStringAsync();
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
                return await DeserializeResponseAsync<TResponse>(response);
            }
        }

        public async Task<TResponse> PutAsync<TContent, TResponse>(Uri requestUri, TContent content)
        {
            using (var response = await PutInternalAsync(requestUri, content))
            {
                return await DeserializeResponseAsync<TResponse>(response);
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
