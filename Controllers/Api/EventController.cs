using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Web.Http;
using Data.Constants;
using Data.Crates;
using StructureMap;
using Data.Crates.Helpers;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Hub.Services;
using Newtonsoft.Json;
using System.Xml.Linq;
using Data.Interfaces.Manifests;
using Hangfire;

namespace HubWeb.Controllers
{
    /// <summary>
    /// Central logging Events controller
    /// </summary>
    public class EventController : ApiController
    {
        private readonly IEvent _event;
        private readonly ICrateManager _crate;
        private IJobDispatcher _jobDispatcher;

        private delegate void EventRouter(LoggingDataCm loggingDataCm);

        public EventController()
        {
            _event = ObjectFactory.GetInstance<IEvent>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _jobDispatcher = ObjectFactory.GetInstance<IJobDispatcher>();
        }

        private EventRouter GetEventRouter(EventCM eventCm)
        {
            if (eventCm.EventName.Equals("Terminal Incident"))
            {
                return _event.HandleTerminalIncident;
            }

            if (eventCm.EventName.Equals("Terminal Event"))
            {
                return _event.HandleTerminalEvent;
            }

            throw new InvalidOperationException("Unknown EventDTO with name: " + eventCm.EventName);
        }

        [HttpPost]
        [ActionName("gen1_event")]
        public IHttpActionResult ProcessGen1Event(CrateDTO submittedEventsCrate)
        {
            var eventCm = _crate.FromDto(submittedEventsCrate).Get<EventCM>();

            if (eventCm.CrateStorage == null)
            {
                return Ok();
            }

            //Request of alex to keep things simple for now
            if (eventCm.CrateStorage.Count != 1)
            {
                throw new InvalidOperationException("Only single crate can be processed for now.");
            }

            EventRouter currentRouter = GetEventRouter(eventCm);

            var errorMsgList = new List<string>();
            foreach (var crateDTO in eventCm.CrateStorage)
            {
                if (crateDTO.ManifestType.Id != (int)MT.LoggingData)
                {
                    errorMsgList.Add("Don't know how to process an EventReport with the Contents: " + JsonConvert.SerializeObject(_crate.ToDto(crateDTO)));
                    continue;
                }

                var loggingData = crateDTO.Get<LoggingDataCm>();
                currentRouter(loggingData);
            }

            if (errorMsgList.Count > 0)
            {
                throw new InvalidOperationException(String.Join(";;;", errorMsgList));
            }

            return Ok();

        }

        public static void ProcessEventsInternal(CrateDTO raw)
        {
            var curCrateStandardEventReport = ObjectFactory.GetInstance<ICrateManager>().FromDto(raw);
            var eventTask = ObjectFactory.GetInstance<IEvent>().ProcessInboundEvents(curCrateStandardEventReport);
            Task.WaitAll(eventTask);
        }

        [HttpPost]
        [ActionName("processevents")]
        public async Task<IHttpActionResult> ProcessEvents(CrateDTO raw)
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

            if (eventReportMS.EventPayload == null)
            {
                throw new ArgumentException("EventReport can't have a null payload");
            }
            if (eventReportMS.ExternalAccountId == null)
            {
                throw new ArgumentException("EventReport can't have a null ExternalAccountId");
            }

            _jobDispatcher.Enqueue(() => ProcessEventsInternal(raw));
            return Ok();
        }
    }
}