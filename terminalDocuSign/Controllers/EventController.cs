using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using System.Net;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using StructureMap;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalDocuSign.Controllers
{
    [RoutePrefix("terminals/terminalDocuSign")]
    public class EventController : ApiController
    {
        private readonly IEvent _event;
        private readonly IHubEventReporter _reporter;
        private readonly DocuSignPolling _polling;
        private readonly IContainer _container;

        public EventController(IEvent @event, IHubEventReporter reporter, DocuSignPolling polling, IContainer container)
        {
            _event = @event;
            _reporter = reporter;
            _polling = polling;
            _container = container;
        }

        [HttpPost]
        [Route("events")]
        public async Task<IHttpActionResult> ProcessIncomingNotification()
        {
            string eventPayLoadContent = await Request.Content.ReadAsStringAsync();
            Debug.WriteLine($"Processing event request {eventPayLoadContent}");

            await _reporter.Broadcast(await _event.Process(_container, eventPayLoadContent));

            return Ok("Processed DocuSign event notification successfully.");
        }

        [HttpPost]
        [Route("polling_notifications")]
        public async Task<PollingDataDTO> ProcessPollingRequest(PollingDataDTO pollingData)
        {
            return await _polling.Poll(pollingData);
        }
    }
}
