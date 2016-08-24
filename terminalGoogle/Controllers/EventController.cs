using System;
using System.Threading.Tasks;
using System.Web.Http;
using StructureMap;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;

namespace terminalGoogle.Controllers
{
    [RoutePrefix("terminals/terminalGoogle")]
    public class EventController : ApiController
    {
        private readonly IHubEventReporter _eventReporter;
        private readonly IEvent _event;
        private readonly IGoogleGmailPolling _gmailPolling;
        private readonly IGoogleGDrivePolling _gdrivePolling;
        private readonly IContainer _container;

        public EventController(
            IHubEventReporter eventReporter,
            IGoogleGmailPolling gmailPolling,
            IGoogleGDrivePolling gdrivePolling,
            IContainer container)
        {
            _eventReporter = eventReporter;
            _event = new Event();
            _gmailPolling = gmailPolling;
            _gdrivePolling = gdrivePolling;
            _container = container;
        }

        [HttpPost]
        [Route("events")]
        public async Task ProcessIncomingNotification()
        {
            string eventPayLoadContent = await Request.Content.ReadAsStringAsync();

            await _eventReporter.Broadcast(await _event.Process(_container, eventPayLoadContent));
        }

        [HttpPost]
        [Route("polling_notifications")]
        public async Task<PollingDataDTO> ProcessPollingRequest([FromBody]PollingDataDTO pollingData)
        {
            GDrivePollingType gDrivePollingType;

            if (string.IsNullOrEmpty(pollingData.AdditionToJobId))
            {
                return await _gmailPolling.Poll(pollingData);
            }
            else if (Enum.TryParse<GDrivePollingType>(pollingData.AdditionToJobId, out gDrivePollingType))
            {
                return await _gdrivePolling.Poll(pollingData, gDrivePollingType);
            }
            else
            {
                throw new ApplicationException("Unable to process polling request: invalid polling object type");
            }
        }
    }
}