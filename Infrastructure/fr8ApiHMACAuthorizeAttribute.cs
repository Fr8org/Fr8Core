using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Data.Interfaces.DataTransferObjects;
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
using StructureMap;
using Hub.Interfaces;
using HubWeb.Infrastructure;

namespace HubWeb
{
    public class fr8ApiHMACAuthorizeAttribute : Attribute, IAuthenticationFilter
    {
        private const int MaxAllowedLatency = 60;
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public bool AllowMultiple
        {
            get
            {
                return false;
            }
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
            string[] authenticationParameters = tokenString.Split(new char[] { ':' });
            

            if (authenticationParameters.Length != 4)
            {
                return false;
            }
            
            var terminalId = int.Parse(authenticationParameters[0]);
            var authToken = authenticationParameters[1];
            var nonce = authenticationParameters[2];
            var requestTime = long.Parse(authenticationParameters[3]);
            //var userId = authenticationParameters[4];

            //Check for ReplayRequests starts here
            //Check if the nonce is already used
            if (MemoryCache.Default.Contains(nonce))
            {
                return false;
            }

            //Check if the maximum allowed request time gap is exceeded,
            if ((DateTime.UtcNow - DateTimeFromUnixTimestampSeconds(requestTime)).Seconds > MaxAllowedLatency)
            {
                return false;
            }

            var terminalService = ObjectFactory.GetInstance<ITerminal>();
            var terminal = await terminalService.GetTerminalById(terminalId);
            if (terminal == null)
            {
                return false;
            }

            /*
            if (string.IsNullOrEmpty(userId))
            {
                //hmm think about this
                //TODO with a empty userId a terminal can only call single Controller
                //which is OpsController
            }

            //let's check if user allowed this terminal to modify it's data
            if (! await terminalService.IsUserSubscribedToTerminal(terminalId, userId))
            {
                return false;
            }
            */
            //Add the nonce to the cache
            MemoryCache.Default.Add(nonce, requestTime, DateTimeOffset.UtcNow.AddSeconds(MaxAllowedLatency));

            //Check for ReplayRequests ends here
            string reformedAuthenticationToken = String.Format("{0}{1}{2}{3}{4}{5}", terminalId,
               HttpUtility.UrlEncode(request.RequestUri.ToString().ToLowerInvariant()), request.Method.Method,
                  requestTime,nonce, await GetContentBase64String(request.Content)/*, userId*/);

            //Each terminal should have be configured with a unique shared key on Hub
            var secretKeyBytes = Convert.FromBase64String(terminal.Secret);
            var authenticationTokenBytes = Encoding.UTF8.GetBytes(reformedAuthenticationToken);

            //Check for data integrity and tampering
            using (var hmac = new HMACSHA512(secretKeyBytes))
            {
                var hashedBytes = hmac.ComputeHash(authenticationTokenBytes);
                string reformedTokenBase64String = Convert.ToBase64String(hashedBytes);

                if (!authToken.Equals(reformedTokenBase64String, StringComparison.OrdinalIgnoreCase))
                { 
                    return false;
                }
            }

            //HttpContext.Current.User = new TerminalPrinciple(terminalId, userId, null);

            return true;
        }

        private async Task<string> GetContentBase64String(HttpContent content)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes;
                if (content != null)
                {
                    bytes = await content.ReadAsByteArrayAsync();
                }
                else
                {
                    bytes = new byte[]{};
                }

                var md5Hash = md5.ComputeHash(bytes);
                return Convert.ToBase64String(md5Hash);
            }
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