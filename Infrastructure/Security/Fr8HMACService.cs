using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Fr8Infrastructure.Interfaces;
using StructureMap;

namespace Fr8Infrastructure.Security
{
    public class Fr8HMACService : IHMACService
    {
        private readonly MediaTypeFormatter _formatter;
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public Fr8HMACService()
        {
            _formatter = ObjectFactory.GetInstance<MediaTypeFormatter>();
        }

        private async Task<string> GetHMACHash(Uri requestUri, string userId, string terminalId, string terminalSecret, string timeStamp, string nonce, string contentBase64String)
        {
            string url = WebUtility.UrlEncode(requestUri.ToString().ToLowerInvariant());
            //Formulate the keys used in plain format as a concatenated string.
            string authenticationKeyString = string.Format("{0}{1}{2}{3}{4}{5}", terminalId, url, timeStamp, nonce, contentBase64String, userId);


            var secretKeyBase64ByteArray = Encoding.ASCII.GetBytes(terminalSecret);//Convert.FromBase64String(terminalSecret);

            using (var hmac = new HMACSHA512(secretKeyBase64ByteArray))
            {
                byte[] authenticationKeyBytes = Encoding.UTF8.GetBytes(authenticationKeyString);
                byte[] authenticationHash = hmac.ComputeHash(authenticationKeyBytes);
                return Convert.ToBase64String(authenticationHash);
            }
        }


        public async Task<string> CalculateHMACHash(Uri requestUri, string userId, string terminalId, string terminalSecret, string timeStamp, string nonce)
        {
            return await CalculateHMACHash(requestUri, userId, terminalId, terminalSecret, timeStamp, nonce, await GetContentBase64String(null));
        }

        public async Task<string> CalculateHMACHash<T>(Uri requestUri, string userId, string terminalId, string terminalSecret, string timeStamp, string nonce, T content)
        {
            return await CalculateHMACHash(requestUri, userId, terminalId, terminalSecret, timeStamp, nonce, await GetContentBase64String(new ObjectContent(typeof(T), content, _formatter)));
        }

        public async Task<string> CalculateHMACHash(Uri requestUri, string userId, string terminalId, string terminalSecret, string timeStamp, string nonce, HttpContent content)
        {
            return await CalculateHMACHash(requestUri, userId, terminalId, terminalSecret, timeStamp, nonce, await GetContentBase64String(content));
        }

        private async Task<string> CalculateHMACHash(Uri requestUri, string userId, string terminalId, string terminalSecret, string timeStamp, string nonce, string contentBase64String)
        {
            return await GetHMACHash(requestUri, userId, terminalId, terminalSecret, timeStamp, nonce, contentBase64String);
        }

        public static long GetCurrentUnixTimestampSeconds()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }

        private async Task<Dictionary<string, string>> GetHMACHeader(string hash, string userId, string terminalId, string timeStamp, string nonce)
        {
            var mergedData = string.Format("{0}:{1}:{2}:{3}:{4}", terminalId, hash, nonce, timeStamp, userId);
            return new Dictionary<string, string>()
            {
                {System.Net.HttpRequestHeader.Authorization.ToString(), string.Format("hmac {0}", mergedData)}
            };
        }

        public async Task<Dictionary<string, string>> GenerateHMACHeader(Uri requestUri, string terminalId, string terminalSecret, string userId, HttpContent content)
        {
            var timeStamp = GetCurrentUnixTimestampSeconds().ToString(CultureInfo.InvariantCulture);
            var nonce = Guid.NewGuid().ToString();
            var hash = await CalculateHMACHash(requestUri, userId, terminalId, terminalSecret, timeStamp, nonce, content);
            return await GetHMACHeader(hash, userId, terminalId, timeStamp, nonce);
        }

        public async Task<Dictionary<string, string>> GenerateHMACHeader<T>(Uri requestUri, string terminalId, string terminalSecret, string userId, T content)
        {
            var timeStamp = GetCurrentUnixTimestampSeconds().ToString(CultureInfo.InvariantCulture);
            var nonce = Guid.NewGuid().ToString();
            var hash = await CalculateHMACHash(requestUri, userId, terminalId, terminalSecret, timeStamp, nonce, content);
            return await GetHMACHeader(hash, userId, terminalId, timeStamp, nonce);
        }

        public async Task<Dictionary<string, string>> GenerateHMACHeader(Uri requestUri, string terminalId, string terminalSecret, string userId)
        {
            var timeStamp = GetCurrentUnixTimestampSeconds().ToString(CultureInfo.InvariantCulture);
            var nonce = Guid.NewGuid().ToString();
            var hash = await CalculateHMACHash(requestUri, userId, terminalId, terminalSecret, timeStamp, nonce);
            return await GetHMACHeader(hash, userId, terminalId, timeStamp, nonce);
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
    }
}
