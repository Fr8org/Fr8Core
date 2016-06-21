using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;
using terminalBasecamp.Infrastructure;

namespace terminalBasecamp.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IBasecampAuthorization _basecampAuthorization;
        private readonly IHubEventReporter _eventReporter;

        public AuthenticationController(IHubEventReporter eventReporter, IBasecampAuthorization basecampAuthorization)
        {
            if (eventReporter == null)
            {
                throw new ArgumentNullException(nameof(eventReporter));
            }
            if (basecampAuthorization == null)
            {
                throw new ArgumentNullException(nameof(basecampAuthorization));
            }
            _eventReporter = eventReporter;
            _basecampAuthorization = basecampAuthorization;
        }

        [HttpPost]
        [Route("request_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            return _basecampAuthorization.GetExternalAuthUrl();
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(ExternalAuthenticationDTO externalAuthDTO)
        {
            return await _basecampAuthorization.AuthenticateAsync(externalAuthDTO);
        }
    }
}