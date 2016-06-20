using System.Threading.Tasks;
using System.Web.Http;
using Fr8.TerminalBase.Services;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using Fr8.Infrastructure.Data.DataTransferObjects;
using StructureMap;
using Fr8.TerminalBase.Interfaces;

namespace terminalGoogle.Controllers
{
    [RoutePrefix("terminals/terminalGoogle")]
    public class EventController : ApiController
    {
        private readonly IHubEventReporter _eventReporter;
        private readonly IEvent _event;
        private readonly IGoogleGmailPolling _gmailPolling;
        private readonly IContainer _container;

        public EventController(IHubEventReporter eventReporter, IGoogleGmailPolling gmailPolling, IContainer container)
        {
            _eventReporter = eventReporter;
            _event = new Event();
            _gmailPolling = gmailPolling;
            _container = container;
        }

        [HttpPost]
        [Route("events")]
        public async Task ProcessIncomingNotification()
        {
            string eventPayLoadContent = await Request.Content.ReadAsStringAsync();

            await _eventReporter.Broadcast(await _event.Process(eventPayLoadContent));
        }

        [HttpPost]
        [Route("polling_notifications")]
        public async Task<PollingDataDTO> ProcessPollingRequest([FromBody]PollingDataDTO pollingData)
        {
            var hubCommunicator = _container.GetInstance<IHubCommunicator>();
            hubCommunicator.Authorize(pollingData.Fr8AccountId);
            pollingData = await _gmailPolling.Poll(hubCommunicator, pollingData);
            return pollingData;
        }
    }
}