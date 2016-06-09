using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.BaseClasses;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : BaseTerminalController
    {
        private const string curTerminal = "terminalQuickBooks";

        private readonly IAuthenticator _authenticator;

        public AuthenticationController(IAuthenticator authenticator, IRestfulServiceClient restfulServiceClient)
            :base (restfulServiceClient)
        {
            _authenticator = authenticator;
        }

        [HttpPost]
        [Route("initial_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            var url = _authenticator.CreateAuthUrl();

            var externalAuthUrlDTO = new ExternalAuthUrlDTO()
            {
                ExternalStateToken = null,
                Url = url
            };

            return externalAuthUrlDTO;
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(
            ExternalAuthenticationDTO externalAuthDTO)
        {
            try
            {
                var query = HttpUtility.ParseQueryString(externalAuthDTO.RequestQueryString);
                var oAuthToken = query["oauth_token"];
                var oAuthVerifier = query["oauth_verifier"];
                var realmId = query["realmId"];
                var dataSource = query["dataSource"];

                if (string.IsNullOrEmpty(oAuthToken) 
                    || string.IsNullOrEmpty(oAuthVerifier) 
                    || string.IsNullOrEmpty(realmId) 
                    || string.IsNullOrEmpty(dataSource))
                {
                    throw new ApplicationException("OAuth Token or OAuth Verifier or Realm ID or Data Source is empty.");
                }

                var authToken = await _authenticator.GetAuthToken(oAuthToken, oAuthVerifier, realmId);
                return authToken;

            }
            catch (Exception ex)
            {
                ReportTerminalError(curTerminal, ex, externalAuthDTO.Fr8UserId);

                return new AuthorizationTokenDTO()
                {
                    Error = "An error occurred while trying to authorize, please try again later."
                };
            }
        }
    }
}