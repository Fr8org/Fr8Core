using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json.Linq;
using terminalAsana.Interfaces;

namespace terminalAsana.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IAsanaOAuth _asanaOAuth;
        private readonly IHubEventReporter _eventReporter;

        public AuthenticationController(IAsanaOAuth asanaOAuth, IHubEventReporter eventReporter)
        {
            _asanaOAuth = asanaOAuth;        
            _eventReporter = eventReporter;

        }

        [HttpPost]
        [Route("request_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            var externalStateToken = Guid.NewGuid().ToString();
            var url = _asanaOAuth.CreateAuthUrl(externalStateToken);

            var externalAuthUrlDTO = new ExternalAuthUrlDTO()
            {
                ExternalStateToken = externalStateToken,
                Url = url
            };

            return externalAuthUrlDTO;
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(ExternalAuthenticationDTO externalAuthDTO)
        {
            try
            {
                string code;
                string state;

                ParseCodeAndState(externalAuthDTO.RequestQueryString, out code, out state);

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                {
                    throw new ApplicationException("Code or State is empty.");
                }

                var oauthTokenData = await _asanaOAuth.GetOAuthTokenDataAsync(code);
                var userInfo = oauthTokenData.Value<JObject>("data");
                var secondsToExpiration = oauthTokenData.Value<int>("expires_in");
                var expirationDate = _asanaOAuth.CalculateExpirationTime(secondsToExpiration);

                return new AuthorizationTokenDTO
                {
                    Token = oauthTokenData.ToString(),
                    ExternalAccountId = userInfo.Value<string>("id"),
                    ExternalAccountName = userInfo.Value<string>("name"),
                    ExternalStateToken = state,
                    ExpiresAt = expirationDate,
                    AdditionalAttributes = expirationDate.ToString("O")
                };
            }
            catch (Exception ex)
            {
                await _eventReporter.ReportTerminalError(ex, externalAuthDTO.Fr8UserId);

                return new AuthorizationTokenDTO()
                {
                    Error = "An error occurred while trying to authorize, please try again later."
                };
            }
        }

        private void ParseCodeAndState(string queryString, out string code, out string state)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                throw new ApplicationException("QueryString is empty.");
            }

            code = null;
            state = null;

            var tokens = queryString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var nameValueTokens = token.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameValueTokens.Length < 2)
                {
                    continue;
                }

                if (nameValueTokens[0] == "code")
                {
                    code = nameValueTokens[1];
                }
                else if (nameValueTokens[0] == "state")
                {
                    state = nameValueTokens[1];
                }
            }
        }
    }
}