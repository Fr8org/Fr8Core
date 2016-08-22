using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IAuthenticator _authenticator;
        private readonly IHubLoggerService _loggerService;

        public AuthenticationController(IAuthenticator authenticator, IHubLoggerService loggerService)
        {
            _loggerService = loggerService;
            _authenticator = authenticator;
        }
        
        [HttpPost]
        [Route("request_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            var state = Guid.NewGuid();
            var url = _authenticator.CreateAuthUrl(state);

            var externalAuthUrlDTO = new ExternalAuthUrlDTO()
            {
                ExternalStateToken = state.ToString(),
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
                var query = HttpUtility.ParseQueryString(externalAuthDTO.RequestQueryString);
                var oAuthToken = query["oauth_token"];
                var oAuthVerifier = query["oauth_verifier"];
                var realmId = query["realmId"];
                var state = query["state"];
                var dataSource = query["dataSource"];

                if (string.IsNullOrEmpty(oAuthToken) 
                    || string.IsNullOrEmpty(oAuthVerifier) 
                    || string.IsNullOrEmpty(realmId) 
                    || string.IsNullOrEmpty(dataSource))
                {
                    throw new ApplicationException("OAuth Token or OAuth Verifier or Realm ID or Data Source is empty.");
                }

                var authToken = await _authenticator.GetAuthToken(oAuthToken, oAuthVerifier, realmId, state);
                return authToken;

            }
            catch (Exception ex)
            {
                await _loggerService.ReportTerminalError(ex, externalAuthDTO.Fr8UserId);

                return new AuthorizationTokenDTO()
                {
                    Error = "An error occurred while trying to authorize, please try again later."
                };
            }
        }
    }
}