using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using terminalGoogle.Services.Authorization;
using TerminalBase.BaseClasses;

namespace terminalGoogle.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : BaseTerminalController
    {
        private const string CurTerminal = "terminalGoogle";

        private readonly IGoogleIntegration _google;


        public AuthenticationController()
        {
            _google = new GoogleIntegration();
        }

        [HttpPost]
        [Route("initial_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            var externalStateToken = Guid.NewGuid().ToString();
            var url = _google.CreateOAuth2AuthorizationUrl(externalStateToken);

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
                var query = HttpUtility.ParseQueryString(externalAuthDTO.RequestQueryString);
                string code = query["code"];
                string state = query["state"];

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                {
                    throw new ApplicationException("Code or State is empty.");
                }

                var oauthToken = _google.GetToken(code);
                var email = await _google.GetExternalUserId(oauthToken);

                return new AuthorizationTokenDTO()
                {
                    Token = JsonConvert.SerializeObject(oauthToken),
                    ExternalStateToken = state,
                    ExternalAccountId = email
                };
            }
            catch (Exception ex)
            {
                ReportTerminalError(CurTerminal, ex);

                return new AuthorizationTokenDTO()
                {
                    Error = "An error occurred while trying to authorize, please try again later."
                };
            }
        }
    }
}