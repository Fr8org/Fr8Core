using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.Services;
using terminalSalesforce.Infrastructure;

namespace terminalSalesforce.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly Authentication _authentication = new Authentication();
        private readonly IHubLoggerService _loggerService;

        public AuthenticationController(IHubLoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpPost]
        [Route("request_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            return _authentication.GetExternalAuthUrl();
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(
            ExternalAuthenticationDTO externalAuthDTO)
        {
            try
            {
                return _authentication.Authenticate(externalAuthDTO);
            }
            catch (Exception ex)
            {
                //The event reporting mechanism does not give the actual error message and it has been commented out in the BaseTerminal#ReportTerminalError
                //Logging explicitly to log4net to see the logs in the App Insights.
                //Logger.GetLogger().Error("Terminal SalesForce Authentication error happened. The error message is " + ex.Message);
                Logger.GetLogger().Error($"Terminal SalesForce Authentication error happened. Fr8UserId = {externalAuthDTO.Fr8UserId} The error message is {ex.Message} ");

                //Report the terminal error in the standard Fr8 Event Reporting mechanism
                await _loggerService.ReportTerminalError(ex, externalAuthDTO.Fr8UserId);

                return new AuthorizationTokenDTO
                    {
                        Error = string.Format("An error occured ({0}) while trying to authenticate, please try again later.", ex.Message)
                    };

            }
        }
    }
}