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

namespace HubWeb
{
    public class fr8ApiHMACAuthorizeAttribute : Attribute, IAuthenticationFilter
    {
        private string _secretKey = "MYSECRETKEY1";
        private const int MaxAllowedLatency = 600;

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

            var terminalId = new Guid(authenticationParameters[0]);
            var authToken = authenticationParameters[1];
            var nonce = authenticationParameters[2];
            var requestTime = authenticationParameters[3];

            var terminalService = ObjectFactory.GetInstance<ITerminal>();
            var terminal = await terminalService.GetTerminalById(terminalId);
            if (terminal == null)
            {
                return false;
            }

            //Check for ReplayRequests starts here
            //Check if the nonce is already used
            if (MemoryCache.Default.Contains(nonce))
            { 
                return false;
            }

            //Check if the maximum allowed request time gap is exceeded,
            if ((DateTime.UtcNow - Convert.ToDateTime(requestTime)).Seconds > MaxAllowedLatency)
            { 
                return false;
            }

            //Add the nonce to the cache
            MemoryCache.Default.Add(nonce, requestTime, DateTimeOffset.UtcNow.AddSeconds(MaxAllowedLatency));
            
            //Check for ReplayRequests ends here
            string reformedAuthenticationToken = String.Format("{0}{1}{2}{3}{4}{5}", terminalId, request.Method.Method,
               HttpUtility.UrlEncode(request.RequestUri.ToString().ToLowerInvariant()),
                  nonce, requestTime, await GetContentBase64String(request.Content));

            //Each AppId should have be configured with a unique shared key on
            //the server!!! Hard coded in the example for simplicity.
            var secretKeyBytes = Convert.FromBase64String(terminal.Secret);
            var authenticationTokenBytes = Encoding.UTF8.GetBytes(reformedAuthenticationToken);

            //Check for data integrity and tampering
            using (HMACSHA512 hmac = new HMACSHA512(secretKeyBytes))
            {
                var hashedBytes = hmac.ComputeHash(authenticationTokenBytes);
                string reformedTokenBase64String = Convert.ToBase64String(hashedBytes);

                if (!authToken.Equals(reformedAuthenticationToken, StringComparison.OrdinalIgnoreCase))
                { 
                    return false;
                }
            }

            return true;
        }

        private async Task<string> GetContentBase64String(HttpContent content)
        {
            using (MD5 md5 = MD5.Create())
            {
                var bytes = await content.ReadAsByteArrayAsync();
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