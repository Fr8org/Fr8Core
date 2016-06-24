using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities.Logging;
using StructureMap;
using Hub.Interfaces;
using HubWeb.Infrastructure_HubWeb;

namespace HubWeb.Controllers
{
    /// <summary>
    /// Central logging Events controller
    /// </summary>
    public class EventsController : ApiController
    {
        private readonly IEvent _event;
        private readonly ICrateManager _crate;
        private IJobDispatcher _jobDispatcher;

        private delegate void EventRouter(LoggingDataCM loggingDataCm);

        public EventsController()
        {
            _event = ObjectFactory.GetInstance<IEvent>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _jobDispatcher = ObjectFactory.GetInstance<IJobDispatcher>();
        }

        public static Task ProcessEventsInternal(CrateDTO raw)
        {
            var curCrateStandardEventReport = ObjectFactory.GetInstance<ICrateManager>().FromDto(raw);
            return ObjectFactory.GetInstance<IEvent>().ProcessInboundEvents(curCrateStandardEventReport);
        }

        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Post(CrateDTO raw)
        {
            //check if its not null
            if (raw == null)
                throw new ArgumentNullException("Parameter Standard Event Report is null.");

            var curCrateStandardEventReport = _crate.FromDto(raw);

            //check if Standard Event Report inside CrateDTO
            if (!curCrateStandardEventReport.IsOfType<EventReportCM>())
                throw new ArgumentNullException("CrateDTO passed is not a Standard Event Report.");

            if (curCrateStandardEventReport.Get() == null)
                throw new ArgumentNullException("CrateDTO Content is empty.");

            var eventReportMS = curCrateStandardEventReport.Get<EventReportCM>();
            Logger.LogInfo($"Crate {raw.Id} with incoming event '{eventReportMS.EventNames}' is received for external account '{eventReportMS.ExternalAccountId}'");

            if (eventReportMS.EventPayload == null)
            {
                throw new ArgumentException("EventReport can't have a null payload");
            }
            if (string.IsNullOrEmpty(eventReportMS.ExternalAccountId) && string.IsNullOrEmpty(eventReportMS.ExternalDomainId))
            {
                throw new ArgumentException("EventReport can't have both ExternalAccountId and ExternalDomainId empty");
            }

            _jobDispatcher.Enqueue(() => ProcessEventsInternal(raw));
            return Ok();
        }
    }
}