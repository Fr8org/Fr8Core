using System.Threading.Tasks;
using System.Web.Http;
using Fr8.TerminalBase.Services;
using terminalInstagram.Models;

namespace terminalInstagram.Controllers
{
    [RoutePrefix("terminals/terminalInstagram")]
    public class EventController : ApiController
    {
        private readonly IHubEventReporter _eventReporter;

        public EventController(IHubEventReporter eventReporter)
        {
            _eventReporter = eventReporter;
        }

        [HttpGet]
        [Route("subscribe")]
        public IHttpActionResult ConfirmSubscription(VerificationMessage msg)
        {
            if (msg.VerifyToken == "fr8facebookeventverification")
            {
                return Ok(msg.Challenge);
            }
            return Ok("Unknown verification call");
        }

        [HttpPost]
        [Route("subscribe")]
        public string GetMedia(string mode, string challenge, string verify_token)
        {
            return challenge;
        }
    }
}
