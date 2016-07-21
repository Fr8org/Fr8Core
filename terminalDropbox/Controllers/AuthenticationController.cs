using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;
using terminalDropbox.Infrastructure;

namespace terminalDropbox.Controllers
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
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(ExternalAuthenticationDTO externalAuthDTO)
        {
            try
            {
                return await _authentication.Authenticate(externalAuthDTO);
            }
            catch (Exception ex)
            {
                await _loggerService.ReportTerminalError(ex, externalAuthDTO.Fr8UserId);
                return await Task.FromResult(
                    new AuthorizationTokenDTO()
                    {
                        Error = "An error occurred while trying to authorize, please try again later."
                    }
                );
            }
        }
    }
}