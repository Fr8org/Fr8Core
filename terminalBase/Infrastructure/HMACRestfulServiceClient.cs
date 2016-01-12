using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Utilities.Configuration.Azure;

namespace TerminalBase.Infrastructure
{
    public class HMACRestfulServiceClient : RestfulServiceClient
    {

        private readonly string _terminalSecret;
        private readonly string _terminalId;

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public HMACRestfulServiceClient()
        {
            _terminalSecret = CloudConfigurationManager.GetSetting("TerminalSecret");
            _terminalId = CloudConfigurationManager.GetSetting("TerminalId");
        }

        public static long GetCurrentUnixTimestampSeconds()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }

        private async Task<string> GetContentBase64String(HttpContent httpContent)
        {
            byte[] content;
            if (httpContent != null)
            {
                content = await httpContent.ReadAsByteArrayAsync();
            }
            else
            {
                content = new byte[] { };
            }

            byte[] contentMd5Hash;
            using (var md5 = MD5.Create())
            {
                contentMd5Hash = md5.ComputeHash(content);
            }
            return Convert.ToBase64String(contentMd5Hash);
        }

        private string GetHeaderValue(HttpRequestHeaders headers, string key)
        {
            var valueList = headers.FirstOrDefault(h => h.Key == key).Value;
            if (valueList != null)
            {
                return valueList.FirstOrDefault();
            }

            return null;
        }

        private async Task AddHMACHeader(HttpRequestMessage request)
        {
            string url = HttpUtility.UrlEncode(request.RequestUri.ToString().ToLowerInvariant());
            string methodName = request.Method.Method;
            string timeStamp = GetCurrentUnixTimestampSeconds().ToString(CultureInfo.InvariantCulture);
            string nonce = Guid.NewGuid().ToString();
            string contentBase64String = await GetContentBase64String(request.Content);
            //hmm think of a better way to pass userId?
            string userId = GetHeaderValue(request.Headers, "fr8UserId") ?? "null";

            //Formulate the keys used in plain format as a concatenated string.
            string authenticationKeyString = string.Format("{0}{1}{2}{3}{4}{5}{6}", _terminalId, url, methodName, timeStamp, nonce, contentBase64String, userId);

            var secretKeyBase64ByteArray = Convert.FromBase64String(_terminalSecret);

            using (var hmac = new HMACSHA512(secretKeyBase64ByteArray))
            {
                byte[] authenticationKeyBytes = Encoding.UTF8.GetBytes(authenticationKeyString);
                byte[] authenticationHash = hmac.ComputeHash(authenticationKeyBytes);
                string hashedBase64String = Convert.ToBase64String(authenticationHash);
                request.Headers.Authorization = new AuthenticationHeaderValue("hmac", string.Format("{0}:{1}:{2}:{3}", _terminalId,
                   hashedBase64String, nonce, timeStamp));
            }
        }

        protected override async Task<HttpResponseMessage> SendInternalAsync(HttpRequestMessage request, string CorrelationId)
        {
            await AddHMACHeader(request);
            return await base.SendInternalAsync(request, CorrelationId);
        }
    }
}
