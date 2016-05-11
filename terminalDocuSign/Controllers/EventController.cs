using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using TerminalBase.Infrastructure;
using StructureMap;
using System.Net;

namespace terminalDocuSign.Controllers
{
    [RoutePrefix("terminals/terminalDocuSign")]
    public class EventController : ApiController
    {
        private IEvent _event;
        private BaseTerminalEvent _baseTerminalEvent;
        private DocuSignPolling _polling;

        public EventController()
        {
            _event = new Event();
            _baseTerminalEvent = new BaseTerminalEvent();
            _polling = ObjectFactory.GetInstance<DocuSignPolling>();
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

        [HttpPost]
        [Route("polling")]
        public async Task<IHttpActionResult> ProcessPollingRequest(string external_account_Id, string fr8AccountId, string polling_interval)
        {
            var result = await _polling.Poll(external_account_Id, fr8AccountId, polling_interval);
            if (result)
                return Ok();
            else
                return Content(HttpStatusCode.Gone, "Polling failed, deschedule it");
        }
    }
}
