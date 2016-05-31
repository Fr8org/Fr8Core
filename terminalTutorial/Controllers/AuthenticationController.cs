using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8Data.DataTransferObjects;
using TerminalBase.BaseClasses;

namespace terminalTutorial.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : BaseTerminalController
    {
        private const string curTerminal = "terminalTutorial";

        [HttpPost]
        [Route("initial_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            return null;
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(ExternalAuthenticationDTO externalAuthDTO)
        {
            return null;
        }
    }
}