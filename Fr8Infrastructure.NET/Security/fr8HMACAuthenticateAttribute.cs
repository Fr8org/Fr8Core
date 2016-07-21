using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using Fr8.Infrastructure.Interfaces;
using StructureMap;

namespace Fr8.Infrastructure.Security
{
    public abstract class fr8HMACAuthenticateAttribute : Attribute, IAuthenticationFilter
    {
        
        private readonly IHMACAuthenticator _hmacAuthenticator;

        protected fr8HMACAuthenticateAttribute()
        {
            _hmacAuthenticator = ObjectFactory.GetInstance<IHMACAuthenticator>();
        }

        public bool AllowMultiple
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// These 3 functions are abstract or virtual
        /// because this class can be used by both terminals and hub
        /// they can implement their custom versions for these
        /// </summary>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        protected abstract Task<string> GetTerminalSecret(string terminalId);

        protected abstract Task<bool> CheckPermission(string terminalId, string userId);

        protected virtual void Success(HttpAuthenticationContext context, string terminalId, string userId)
        {
            return;
        }

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;

            string terminalId,userId = null;

            _hmacAuthenticator.ExtractTokenParts(request, out terminalId, out userId);
            var terminalSecret = await GetTerminalSecret(terminalId);
            var isValid = await _hmacAuthenticator.IsValidRequest(request, terminalSecret);

            if (!isValid)
            {
                return;
            }

            isValid = await CheckPermission(terminalId, userId);
            if (!isValid)
            {
                return;
            }

            Success(context, terminalId, userId);
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