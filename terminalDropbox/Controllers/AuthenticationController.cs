using System;
using System.Threading.Tasks;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using terminalDropbox.Infrastructure;

namespace terminalDropbox.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : BaseTerminalController
    {
        private const string curTerminal = "terminalDropbox";
        private Authentication _authentication = new Authentication();
        
        [HttpPost]
        [Route("initial_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            return _authentication.GetExternalAuthUrl();
        }

        [HttpPost]
        [Route("token")]
        public Task<AuthorizationTokenDTO> GenerateOAuthToken(
         ExternalAuthenticationDTO externalAuthDTO)
        {
            try
            {
                return Task.FromResult(_authentication.Authenticate(externalAuthDTO));
            }
            catch (Exception ex)
            {
                ReportTerminalError(curTerminal, ex);
                return Task.FromResult(
                    new AuthorizationTokenDTO()
                    {
                        Error = "An error occured while trying to authenticate, please try again later."
                    }
                );
            }
        }
    }
}