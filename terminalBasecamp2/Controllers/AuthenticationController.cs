using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;
using terminalBasecamp2.Infrastructure;

namespace terminalBasecamp2.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IBasecampApiClient _basecampApiClient;
        private readonly IHubEventReporter _eventReporter;

        public AuthenticationController(IHubEventReporter eventReporter, IBasecampApiClient basecampApiClient)
        {
            if (eventReporter == null)
            {
                throw new ArgumentNullException(nameof(eventReporter));
            }
            if (basecampApiClient == null)
            {
                throw new ArgumentNullException(nameof(basecampApiClient));
            }
            _eventReporter = eventReporter;
            _basecampApiClient = basecampApiClient;
        }

        [HttpPost]
        [Route("request_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            return _basecampApiClient.GetExternalAuthUrl();
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(ExternalAuthenticationDTO externalAuthDTO)
        {
            return await _basecampApiClient.AuthenticateAsync(externalAuthDTO);
        }
    }
}