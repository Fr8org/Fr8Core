using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using StructureMap;
using terminalFacebook.Interfaces;
using terminalFacebook.Models;

namespace terminalFacebook.Controllers
{
    [RoutePrefix("terminals/terminalFacebook")]
    public class EventController : ApiController
    {
        private readonly IEvent _event;
        private readonly IHubEventReporter _reporter;
        private readonly IContainer _container;
        
        public EventController(IEvent @event, IHubEventReporter reporter, IContainer container)
        {
            _event = @event;
            _reporter = reporter;
            _container = container;
        }

        [HttpPost]
        [Route("usernotifications")]
        public async Task<IHttpActionResult> ProcessIncomingNotification()
        {
            var data = Request.GetQueryNameValuePairs();
            string eventPayLoadContent = await Request.Content.ReadAsStringAsync();
            Debug.WriteLine($"Processing event request for fb: {eventPayLoadContent}");

            /*var eventList = await _event.ProcessUserEvents(_container, eventPayLoadContent);
            foreach (var fbEvent in eventList)
            {
                await _reporter.Broadcast(fbEvent);
            }*/
           return Ok("Processed Facebook event notification successfully.");
        }

        
        [HttpGet]
        [Route("usernotifications")]
        public IHttpActionResult VerifyFacebookWebhookRegistration(VerificationMessage msg)
        {
            if (msg.VerifyToken == "fr8facebookeventverification")
            {
                return Ok(msg.Challenge);
            }
            return Ok("Unknown verification call");
        }
    }

    
}
