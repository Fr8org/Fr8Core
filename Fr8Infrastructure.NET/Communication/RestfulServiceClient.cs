using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Logging;
using System.Net;

namespace Fr8.Infrastructure.Communication
{
    public class RestfulServiceClient : IRestfulServiceClient
    {
        private const string HttpLogFormat = "--New Request--\nIsSuccess: {0}\nFinished: {1}\nTotal Elapsed: {2}\nCorrelation ID: {3}\nHttp Status: {4}({5})\n";
        class FormatterLogger : IFormatterLogger
        {
            public void LogError(string message, Exception ex)
            {
                //Log.Error(message, ex);
                Logger.GetLogger().Error($"{message}. Exception = {ex}");
            }

            public void LogError(string errorPath, string errorMessage)
            {
                //Log.Error(string.Format("{0}: {1}", errorPath, errorMessage))
                Logger.GetLogger().Error($"{errorPath}: {errorMessage}");
            }
        }

        private readonly HttpClient _innerClient;
        private readonly MediaTypeFormatter _formatter;
        private readonly FormatterLogger _formatterLogger;
        private List<IRequestSignature> Signatures { get; set; }

        /// <summary>
        /// Creates an instance with JSON formatter for requests and responses
        /// </summary>
        public RestfulServiceClient()
            : this(new JsonMediaTypeFormatter(), null)
        {
        }

        public RestfulServiceClient(HttpClient _client)
          : this(new JsonMediaTypeFormatter(), _client)
        {
        }

        /// <summary>
        /// Creates an instance with specified formatter for requests and responses
        /// </summary>
        public RestfulServiceClient(MediaTypeFormatter formatter, HttpClient client)
        {
            ServicePointManager.SecurityProtocol = 
                SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls; // to enable the services which require TLS 1.1+ (Salesforce, Asana)

            if (client == null)
            {
                client = new HttpClient();

#if DEBUG 
                client.Timeout = new TimeSpan(0, 10, 0); //10 minutes
#else
                client.Timeout = new TimeSpan(0, 2, 0); //2 minutes
#endif
            }

            _innerClient = client;
            _formatter = formatter;
            _formatterLogger = new FormatterLogger();
            Signatures = new List<IRequestSignature>();
        }

        public void AddRequestSignature(IRequestSignature signature)
        {
            Signatures.Add(signature);
        }

        protected virtual async Task<HttpResponseMessage> SendInternalAsync(HttpRequestMessage request, string CorrelationId)
        {
            HttpResponseMessage response;
            var responseContent = "";
            var statusCode = -1;

            var stopWatch = Stopwatch.StartNew();
            Exception raisedException = null;
            string prettyStatusCode = null;

            foreach (var requestSignature in Signatures)
            {
                requestSignature.SignRequest(request);
            }

            try
            {
                response = await _innerClient.SendAsync(request);
                responseContent = await response.Content.ReadAsStringAsync();
                prettyStatusCode = response.StatusCode.ToString();
                statusCode = (int)response.StatusCode;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                raisedException = ex;
                var responseMessage = ExtractErrorMessage(responseContent);
                string errorMessage = $"An error has ocurred while sending a {request.RequestUri} request to {request.Method.Method}. Response message: {responseMessage}";
                throw new RestfulServiceException(statusCode, errorMessage, responseMessage, ex);
            }
            catch (TaskCanceledException ex)
            {
                raisedException = ex;
                throw new TimeoutException($"Timeout while making HTTP request.  \r\nURL: {request.RequestUri},   \r\nMethod: {request.Method.Method}");
            }
            catch (Exception ex)
            {
                raisedException = ex;
                string errorMessage = $"An error has ocurred while sending a {request.RequestUri} request to {request.Method.Method}.";
                throw new ApplicationException(errorMessage);
            }
            finally
            {
                stopWatch.Stop();
                var isSuccess = raisedException == null;
                //log details
                var logDetails = string.Format(HttpLogFormat, isSuccess ? "YES" : "NO",
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                    stopWatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture),
                    CorrelationId, statusCode, prettyStatusCode);
            }
            return response;
        }

        protected virtual string ExtractErrorMessage(string responseContent)
        {
            return responseContent;
        }

        #region InternalRequestMethods
        private async Task<HttpResponseMessage> GetInternalAsync(Uri requestUri, string CorrelationId, Dictionary<string, string> headers)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                AddHeaders(request, headers);
                return await SendInternalAsync(request, CorrelationId);
            }
        }

        private async Task<HttpResponseMessage> PostInternalAsync(Uri requestUri, string CorrelationId, Dictionary<string, string> headers)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                AddHeaders(request, headers);
                return await SendInternalAsync(request, CorrelationId);
            }
        }

        private async Task<HttpResponseMessage> PostInternalAsync(Uri requestUri, HttpContent content, string CorrelationId, Dictionary<string, string> headers)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content })
            {
                AddHeaders(request, headers);
                return await SendInternalAsync(request, CorrelationId);
            }
        }
        private async Task DeleteInternalAsync(Uri requestUri, string CorrelationId, Dictionary<string, string> headers)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, requestUri))
            {
                AddHeaders(request, headers);
                var response = await SendInternalAsync(request, CorrelationId);
                response.Dispose();
            }
        }
        private async Task<HttpResponseMessage> PutInternalAsync(Uri requestUri, HttpContent content, string CorrelationId, Dictionary<string, string> headers)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, requestUri) { Content = content })
            {
                AddHeaders(request, headers);
                return await SendInternalAsync(request, CorrelationId);
            }
        }

        #endregion

        private void AddHeaders(HttpRequestMessage request, Dictionary<string, string> headers)
        {
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> entry in headers)
                {
                    request.Headers.Add(entry.Key, entry.Value);
                }
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

        #region GenericRequestMethods
        /// <summary>
        /// Downloads file as a MemoryStream from given URL
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="CorrelationId"></param>
        /// <param name="headers"></param>
        /// <returns>MemoryStream</returns>
        public async Task<Stream> DownloadAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            using (var response = await GetInternalAsync(requestUri, CorrelationId, headers))
            {
                //copy stream because response will be disposed on return
                var downloadedFile = await response.Content.ReadAsStreamAsync();
                var copy = new MemoryStream();
                await downloadedFile.CopyToAsync(copy);
                //rewind stream
                copy.Position = 0;
                return copy;
            }
        }

        public async Task<TResponse> GetAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            using (var response = await GetInternalAsync(requestUri, CorrelationId, headers))
            {
                return await DeserializeResponseAsync<TResponse>(response);
            }
        }

        public async Task<string> GetAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            using (var response = await GetInternalAsync(requestUri, CorrelationId, headers))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<TResponse> PostAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            using (var response = await PostInternalAsync(requestUri, CorrelationId, headers))
            {
                return await DeserializeResponseAsync<TResponse>(response);
            }
        }

        public async Task<string> PostAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            using (var response = await PostInternalAsync(requestUri, CorrelationId, headers))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> PostAsync<TContent>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            return await PostAsync(requestUri, (HttpContent)new ObjectContent(typeof(TContent), content, _formatter), CorrelationId, headers);
        }

        public async Task<TResponse> PostAsync<TContent, TResponse>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            return await PostAsync<TResponse>(requestUri, new ObjectContent(typeof(TContent), content, _formatter), CorrelationId, headers);
        }
        public async Task<TResponse> PutAsync<TContent, TResponse>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            return await PutAsync<TResponse>(requestUri, new ObjectContent(typeof(TContent), content, _formatter), CorrelationId, headers);
        }
        public async Task<string> PutAsync<TContent>(Uri requestUri, TContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            return await PutAsync(requestUri, (HttpContent)new ObjectContent(typeof(TContent), content, _formatter), CorrelationId, headers);
        }

        #endregion

        #region HttpContentRequestMethods
        public async Task<string> PostAsync(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            using (var response = await PostInternalAsync(requestUri, content, CorrelationId, headers))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        public async Task<TResponse> PostAsync<TResponse>(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            using (var response = await PostInternalAsync(requestUri, content, CorrelationId, headers))
            {
                return await DeserializeResponseAsync<TResponse>(response);
            }
        }
        public async Task DeleteAsync(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            await DeleteInternalAsync(requestUri, CorrelationId, headers);
        }
        public async Task<TResponse> PutAsync<TResponse>(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            using (var response = await PutInternalAsync(requestUri, content, CorrelationId, headers))
            {
                return await DeserializeResponseAsync<TResponse>(response);
            }
        }
        public async Task<string> PutAsync(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
        {
            using (var response = await PutInternalAsync(requestUri, content, CorrelationId, headers))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        #endregion

    }
}