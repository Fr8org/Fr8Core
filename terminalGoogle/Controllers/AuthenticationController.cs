using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using TerminalBase.BaseClasses;

namespace terminalGoogle.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : BaseTerminalController
    {
        private const string curPlugin = "terminalGoogle";

        private readonly IGoogleSheet _google;


        public AuthenticationController()
        {
            _google = new GoogleSheet();
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
        public AuthTokenDTO GenerateOAuthToken(ExternalAuthenticationDTO externalAuthDTO)
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

                return new AuthTokenDTO()
                {
                    Token = JsonConvert.SerializeObject(oauthToken),
                    ExternalStateToken = state
                };
            }
            catch (Exception ex)
            {
                ReportPluginError(curPlugin, ex);

                return new AuthTokenDTO()
                {
                    Error = "An error occured while trying to authenticate, please try again later."
                };
            }
        }
    }
}