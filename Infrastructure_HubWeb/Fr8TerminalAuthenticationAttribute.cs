using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using Hub.Infrastructure;
using Hub.Interfaces;
using StructureMap;

namespace HubWeb.Infrastructure_HubWeb
{
    /// <summary>
    /// This attribute checks for Fr8-Token authentication header in requests
    /// authenticates terminals to hub
    /// </summary>
    public class Fr8TerminalAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        private readonly ITerminal _terminal;
        public Fr8TerminalAuthenticationAttribute()
        {
            _terminal = ObjectFactory.GetInstance<ITerminal>();
        }

        protected void Success(HttpAuthenticationContext context, string terminalKey)
        {
            var identity = new Fr8Identity(terminalKey);
            var principle = new Fr8Principal(terminalKey, identity, new[] { "Terminal" });
            Thread.CurrentPrincipal = principle;
            context.Principal = principle;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principle;
            }
        }

        private Dictionary<string, string> ExtractTokenParts(HttpRequestMessage request)
        {
            if (request.Headers.Authorization == null || !request.Headers.Authorization.Scheme.Equals("FR8", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                return null;
            }
            string tokenString = request.Headers.Authorization.Parameter;
            string[] authenticationParameters = tokenString.Split(',');
            var headerParams = new Dictionary<string, string>();
            foreach (var authenticationParameter in authenticationParameters)
            {
                var splittedParam = authenticationParameter.Split('=');
                if (splittedParam.Length != 2)
                {
                    return null;
                }
                headerParams.Add(splittedParam[0].Trim(), splittedParam[1].Trim());
            }
            return headerParams;
        }

        public bool AllowMultiple => false;

        /// <summary>
        /// Authenticates terminals by checking fr8 terminal authentication header
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;

            var headerParams = ExtractTokenParts(request);
            string terminalKey = headerParams?.FirstOrDefault(x => x.Key == "terminal_key").Value;
            //unknown terminal
            if (terminalKey == null)
            {
                return;
            }
            var terminal = await _terminal.GetByKey(terminalKey);
            if (terminal == null)
            {
                return;
            }
            Success(context, terminalKey);
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
                    response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("FR8-TOKEN"));
                }

                return response;
            }
        }
    }
}