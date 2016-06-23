using System.Threading.Tasks;
using System.Web.Http;
using Fr8.TerminalBase.Services;
using terminalInstagram.Models;
using System.Linq;

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
        public string ConfirmSubscription(VerificationMessage msg)
        {
            return msg.Challenge;
        }

        [HttpPost]
        [Route("subscribe")]
        public async Task<IHttpActionResult> GetMedia()
        {
            var hash = Request.Headers.GetValues("x-hub-signature").FirstOrDefault();
            string eventPayLoadContent = await Request.Content.ReadAsStringAsync();
            return Ok();
        }
    }
}
