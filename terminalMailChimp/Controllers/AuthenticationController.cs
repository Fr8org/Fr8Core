using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using terminalMailChimp.Interfaces;

namespace terminalMailChimp.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IMailChimpIntegration _mailChimpIntegration;
        private readonly IHubEventReporter _eventReporter;

        public AuthenticationController(IHubEventReporter eventReporter, IMailChimpIntegration mailChimpIntegration)
        {
            _mailChimpIntegration = mailChimpIntegration;
            _eventReporter = eventReporter;
        }

        [HttpPost]
        [Route("request_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            return _mailChimpIntegration.GenerateOAuthInitialUrl();
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

                return await _mailChimpIntegration.GetAuthToken(code, state);
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
    }
}