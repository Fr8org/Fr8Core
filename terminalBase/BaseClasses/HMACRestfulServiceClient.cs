using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Hub.Managers.APIManagers.Transmitters.Restful;

namespace TerminalBase.BaseClasses
{
    public class HMACRestfulServiceClient : RestfulServiceClient
    {
        //TODO read these values from app config
        private readonly string _terminalSecret = "hmactest";
        private readonly string _terminalId = "5";

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public HMACRestfulServiceClient()
        {

        }

        public static long GetCurrentUnixTimestampSeconds()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }

        private async Task AddHMACHeader(HttpRequestMessage request)
        {
            string url = HttpUtility.UrlEncode(request.RequestUri.ToString().ToLowerInvariant());
            string methodName = request.Method.Method;
            string timeStamp = GetCurrentUnixTimestampSeconds().ToString(CultureInfo.InvariantCulture);
            string nonce = Guid.NewGuid().ToString();
            byte[] content;
            if (request.Content != null)
            {
                content = await request.Content.ReadAsByteArrayAsync();
            }
            else
            {
                content = new byte[] {};
            }
            MD5 md5 = MD5.Create();
            byte[] contentMd5Hash = md5.ComputeHash(content);
            string contentBase64String = Convert.ToBase64String(contentMd5Hash);

            //Formulate the keys used in plain format as
            //a concatenated string.
            //Note that the secret key is not provided here.
            string authenticationKeyString = string.Format("{0}{1}{2}{3}{4}{5}", _terminalId, url, methodName, timeStamp, nonce, contentBase64String);

            var secretKeyBase64ByteArray = Convert.FromBase64String(_terminalSecret);

            using (var hmac = new HMACSHA512(secretKeyBase64ByteArray))
            {
                byte[] authenticationKeyBytes = Encoding.UTF8.GetBytes(authenticationKeyString);
                byte[] authenticationHash = hmac.ComputeHash(authenticationKeyBytes);
                string hashedBase64String = Convert.ToBase64String(authenticationHash);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("hmac", string.Format("{0}:{1}:{2}:{3}", _terminalId,
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
