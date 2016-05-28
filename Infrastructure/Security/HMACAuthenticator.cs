using System;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Fr8Infrastructure.Interfaces;
using StructureMap;

namespace Fr8Infrastructure.Security
{
    public class HMACAuthenticator : IHMACAuthenticator
    {
        private const int MaxAllowedLatency = 60;
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly IHMACService _hmacService;
        public HMACAuthenticator()
        {
            _hmacService = ObjectFactory.GetInstance<IHMACService>();
        }

        public void ExtractTokenParts(HttpRequestMessage request, out string terminalId, out string userId)
        {
            terminalId = null;
            userId = null;
            var tokenParts = ExtractTokenParts(request);
            if (tokenParts == null)
            {
                return;
            }
            terminalId = tokenParts[0];
            userId = tokenParts[4];
        }

        private string[] ExtractTokenParts(HttpRequestMessage request)
        {
            if (request.Headers.Authorization == null || !request.Headers.Authorization.Scheme.Equals("hmac", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                return null;
            }

            string tokenString = request.Headers.Authorization.Parameter;
            string[] authenticationParameters = tokenString.Split(new[] { ':' });

            if (authenticationParameters.Length != 5)
            {
                return null;
            }

            return authenticationParameters;
        }

        public async Task<bool> IsValidRequest(HttpRequestMessage request, string terminalSecret)
        {
            var tokenParts = ExtractTokenParts(request);
            if (tokenParts == null)
            {
                return false;
            }

            var terminalId = tokenParts[0];
            var authToken = tokenParts[1];
            var nonce = tokenParts[2];
            var requestTime = tokenParts[3];
            var userId = tokenParts[4];

            //Check for ReplayRequests starts here
            //Check if the nonce is already used
            if (MemoryCache.Default.Contains(nonce))
            {
                return false;
            }

            long requestTimeLong;
            if (!long.TryParse(requestTime, out requestTimeLong))
            {
                return false;
            }

            //Check if the maximum allowed request time gap is exceeded,
            if ((DateTime.UtcNow - DateTimeFromUnixTimestampSeconds(requestTimeLong)).TotalSeconds > MaxAllowedLatency)
            {
                return false;
            }

            //Add the nonce to the cache
            MemoryCache.Default.Add(nonce, requestTime, DateTimeOffset.UtcNow.AddSeconds(MaxAllowedLatency));

            //Check for ReplayRequests ends here

            if (string.IsNullOrEmpty(terminalSecret))
            {
                return false;
            }

            var calculatedHash = await _hmacService.CalculateHMACHash(request.RequestUri, userId, terminalId, terminalSecret, requestTime, nonce, request.Content);
            if (!authToken.Equals(calculatedHash, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            
            return true;
        }

        private static DateTime DateTimeFromUnixTimestampSeconds(long seconds)
        {
            return UnixEpoch.AddSeconds(seconds);
        }
    }
}
