using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Fr8Infrastructure.Interfaces;
using log4net;
using Utilities.Logging;

namespace Fr8Infrastructure.Communication
{
    public class RestfulServiceClient : IRestfulServiceClient
    {
        private static readonly ILog Log = Logger.GetLogger();
        private const string HttpLogFormat = "--New Request--\nIsSuccess: {0}\nFinished: {1}\nTotal Elapsed: {2}\nCorrelation ID: {3}\nHttp Status: {4}({5})\n";
        class FormatterLogger : IFormatterLogger
        {
            public void LogError(string message, Exception ex)
            {
                //Log.Error(message, ex);
                Logger.LogError($"{message}. Exception = {ex}");
            }

            public void LogError(string errorPath, string errorMessage)
            {
                //Log.Error(string.Format("{0}: {1}", errorPath, errorMessage))
                Logger.LogError($"{errorPath}: {errorMessage}");
            }
        }

        private readonly HttpClient _innerClient;
        private readonly MediaTypeFormatter _formatter;
        private readonly FormatterLogger _formatterLogger;

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
            if (client == null)
            {
                client = new HttpClient();

#if DEBUG 
                client.Timeout = new TimeSpan(0, 10, 0); //5 minutes
#else
                client.Timeout = new TimeSpan(0, 2, 0); //2 minutes
#endif
            }

            _innerClient = client;
            _formatter = formatter;
            _formatterLogger = new FormatterLogger();
        }

        protected virtual async Task<HttpResponseMessage> SendInternalAsync(HttpRequestMessage request, string CorrelationId)
        {
            HttpResponseMessage response;
            string responseContent = "";
            int statusCode = -1;

            var stopWatch = Stopwatch.StartNew();
            Exception raisedException = null;
            string prettyStatusCode = null;
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
                string errorMessage = String.Format("An error has ocurred while sending a {0} request to {1}. Response message: {2}",
                    request.RequestUri,
                    request.Method.Method,
                    ExtractErrorMessage(responseContent));
                throw new RestfulServiceException(statusCode, errorMessage, ex);
            }
            catch (TaskCanceledException ex)
            {
                raisedException = ex;
                //Timeout
                throw new TimeoutException(
                    String.Format("Timeout while making HTTP request.  \r\nURL: {0},   \r\nMethod: {1}",
                    request.RequestUri,
                    request.Method.Method));
            }
            catch (Exception ex)
            {
                raisedException = ex;
                string errorMessage = String.Format("An error has ocurred while sending a {0} request to {1}.",
                    request.RequestUri,
                    request.Method.Method);
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

                // This code tends to eat a lot of processor time on my macine and blocks.
                /*if (isSuccess)
                {
                    Log.Info(logDetails);
                }
                else
                {
                    Log.Error(logDetails, raisedException);
                }*/
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
