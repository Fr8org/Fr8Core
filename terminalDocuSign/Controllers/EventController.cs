using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Controllers
{
    [RoutePrefix("terminals/terminalDocuSign")]
    public class EventController : ApiController
    {
        private IEvent _event;
        private BaseTerminalEvent _baseTerminalEvent;

        public EventController()
        {
            _event = new Event();
            _baseTerminalEvent = new BaseTerminalEvent();
        }

        [HttpPost]
        [Route("events")]
        public async Task<IHttpActionResult> ProcessIncomingNotification()
        {
            string eventPayLoadContent = await Request.Content.ReadAsStringAsync();
            Debug.WriteLine($"Processing event request {eventPayLoadContent}");
            await _baseTerminalEvent.Process(eventPayLoadContent, _event.Process);
            return Ok("Processed DocuSign event notification successfully.");
        }
    }
}
