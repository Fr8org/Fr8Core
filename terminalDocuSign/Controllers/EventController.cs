using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using StructureMap;
using System.Net;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Interfaces;

namespace terminalDocuSign.Controllers
{
    [RoutePrefix("terminals/terminalDocuSign")]
    public class EventController : ApiController
    {
        private readonly IEvent _event;
        private readonly BaseTerminalEvent _baseTerminalEvent;
        private readonly DocuSignPolling _polling;
        private readonly IContainer _container;

        public EventController(IEvent @event, BaseTerminalEvent baseTerminalEvent, DocuSignPolling polling, IContainer container)
        {
            _event = @event;
            _baseTerminalEvent = baseTerminalEvent;
            _polling = polling;
            _container = container;
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
        [Route("polling_notifications")]
        public async Task<IHttpActionResult> ProcessPollingRequest(string job_id, string fr8_account_id, string polling_interval)
        {
            var hubCommunicator = _container.GetInstance<IHubCommunicator>();
            hubCommunicator.Configure("terminalDocuSign", fr8_account_id);

            var result = await _polling.Poll(hubCommunicator, job_id, polling_interval);
            if (result)
                return Ok();
            else
                return Content(HttpStatusCode.Gone, "Polling failed, deschedule it");
        }
    }
}
