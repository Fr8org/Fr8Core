using System.Threading.Tasks;
using System.Web.Http;
using Fr8.TerminalBase.Services;
using terminalInstagram.Models;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net;
using StructureMap;
using terminalInstagram.Services;
using terminalInstagram.Interfaces;

namespace terminalInstagram.Controllers
{
    [RoutePrefix("terminals/terminalInstagram")]
    public class EventController : ApiController
    {
        private readonly IHubEventReporter _eventReporter;
        private readonly IInstagramEventManager _event;
        private readonly IContainer _container;

        public EventController(IHubEventReporter eventReporter)
        {
            _eventReporter = eventReporter;
        }

        [HttpGet]
        [Route("subscribe")]
        public HttpResponseMessage ConfirmSubscription(VerificationMessage msg)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(msg.Challenge, Encoding.UTF8, "application/json");
            return response; 

        }
        [HttpPost]
        [Route("subscribe")]
        public async Task<IHttpActionResult> ProcessIncomingMedia()
        {
            var hash = Request.Headers.GetValues("x-hub-signature").FirstOrDefault();
            string eventPayLoadContent = await Request.Content.ReadAsStringAsync();
            var eventList = await _event.ProcessUserEvents(_container, eventPayLoadContent);
            return Ok();
        }
    }
}
