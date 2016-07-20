using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;
    
namespace $safeprojectname$.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IHubEventReporter _eventReporter;

        public AuthenticationController(IHubEventReporter eventReporter)
        {
            if (eventReporter == null)
            {
                throw new ArgumentNullException(nameof(eventReporter));
            }

            _eventReporter = eventReporter;
        }

        [HttpPost]
        [Route("request_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            // Add code which returns URL of the OAuth authorization window
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(ExternalAuthenticationDTO externalAuthDTO)
        {
            // Add code which processes OAuth authorization and returns a OAuth token packaged 
            // into an AuthorizationTokenDTO. See code of the existing terminals 
            // (terminal Google, terminalDropbox) for an example. 
            throw new NotImplementedException();
        }
    }
}