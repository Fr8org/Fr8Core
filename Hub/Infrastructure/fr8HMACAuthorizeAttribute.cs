using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Caching;
using System.Web;
using Hub.Interfaces;
using StructureMap;

namespace Hub.Infrastructure
{
    public abstract class fr8HMACAuthorizeAttribute : Attribute, IAuthenticationFilter
    {
        private const int MaxAllowedLatency = 60;
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly IHMACService _hmacService;

        protected fr8HMACAuthorizeAttribute()
        {
            _hmacService = ObjectFactory.GetInstance<IHMACService>();
        }

        public bool AllowMultiple
        {
            get
            {
                return false;
            }
        }

        protected abstract Task<string> GetTerminalSecret(string terminalId);

        protected abstract Task<bool> CheckAuthentication(string terminalId, string userId);

        protected virtual void Success(string terminalId, string userId)
        {
            return;
        }

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;
            if (!(await IsValidRequest(request)))
            {
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
            }
        }

        private static DateTime DateTimeFromUnixTimestampSeconds(long seconds)
        {
            return UnixEpoch.AddSeconds(seconds);
        }

        private async Task<bool> IsValidRequest(HttpRequestMessage request)
        {
            if (request.Headers.Authorization == null || !request.Headers.Authorization.Scheme.Equals("hmac", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                return false;
            }

            string tokenString = request.Headers.Authorization.Parameter;
            string[] authenticationParameters = tokenString.Split(new [] { ':' });
            

            if (authenticationParameters.Length != 5)
            {
                return false;
            }
            
            var terminalId = authenticationParameters[0];
            var authToken = authenticationParameters[1];
            var nonce = authenticationParameters[2];
            var requestTime = authenticationParameters[3];
            var userId = authenticationParameters[4];

            //Check for ReplayRequests starts here
            //Check if the nonce is already used
            if (MemoryCache.Default.Contains(nonce))
            {
                return false;
            }

            //Check if the maximum allowed request time gap is exceeded,
            if ((DateTime.UtcNow - DateTimeFromUnixTimestampSeconds(long.Parse(requestTime))).Seconds > MaxAllowedLatency)
            {
                return false;
            }

            //Add the nonce to the cache
            MemoryCache.Default.Add(nonce, requestTime, DateTimeOffset.UtcNow.AddSeconds(MaxAllowedLatency));

            //Check for ReplayRequests ends here

            var terminalSecret = await GetTerminalSecret(terminalId);
            if (terminalSecret == null)
            {
                return false;
            }

            var calculatedHash = await _hmacService.CalculateHMACHash(request.RequestUri, userId, terminalId, terminalSecret, requestTime, nonce, request.Content);
            if (!authToken.Equals(calculatedHash, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var status = await CheckAuthentication(terminalId, userId);
            if (status)
            {
                Success(terminalId, userId);
            }

            return status;
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            context.Result = new ResultWithChallenge(context.Result);
            return Task.FromResult(0);
        }


        protected class ResultWithChallenge : IHttpActionResult
        {
            private readonly IHttpActionResult next;

            public ResultWithChallenge(IHttpActionResult next)
            {
                this.next = next;
            }

            public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = await next.ExecuteAsync(cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("hmac"));
                }

                return response;
            }
        }
    }
}