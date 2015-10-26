using System;
using System.Threading.Tasks;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using terminalSalesforce.Infrastructure;

namespace terminalSalesforce.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : BasePluginController
    {
        private const string curPlugin = "terminalSalesforce";
        
        private Authentication _authentication = new Authentication();


        [HttpPost]
        [Route("initial_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            return _authentication.GetExternalAuthUrl();
        }

        [HttpPost]
        [Route("token")]
        public Task<AuthTokenDTO> GenerateOAuthToken(
            ExternalAuthenticationDTO externalAuthDTO)
        {
            try
            {
                return Task.FromResult(_authentication.Authenticate(externalAuthDTO));
            }
            catch (Exception ex)
            {
                ReportPluginError(curPlugin, ex);

                return Task.FromResult(
                    new AuthTokenDTO()
                    {
                        Error = "An error occured while trying to authenticate, please try again later."
                    }
                );
            }
        }
    }
}