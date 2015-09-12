using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using Utilities.Serializers.Json;

namespace Web.Controllers
{
    /// <summary>
    /// Central logging Events controller
    /// </summary>
    public class EventController : ApiController
    {
        private readonly IEvent _event;
        private readonly ICrate _crate;

        private delegate void EventRouter(LoggingData loggingData);

        public EventController()
        {
            _event = ObjectFactory.GetInstance<IEvent>();
            _crate = ObjectFactory.GetInstance<ICrate>();
        }

        private EventRouter GetEventRouter(EventDTO eventDTO)
        {
            if (eventDTO.EventName.Equals("Plugin Incident"))
            {
                return _event.HandlePluginIncident;
            }

            if (eventDTO.EventName.Equals("Plugin Event"))
            {
                return _event.HandlePluginEvent;
            }

            throw new InvalidOperationException("Unknown EventDTO with name: " + eventDTO.EventName);
        }

        [HttpPost]
        public IHttpActionResult Post(CrateDTO submittedEventsCrate)
        {

            var eventDTO = _crate.GetContents<EventDTO>(submittedEventsCrate);

            //Request of alex to keep things simple for now
            if (eventDTO.CrateStorage.Count != 1)
            {
                throw new InvalidOperationException("Only single crate can be processed for now.");
            }

            EventRouter currentRouter = GetEventRouter(eventDTO);

            var errorMsgList = new List<string>();
            foreach (var crateDTO in eventDTO.CrateStorage)
            {
                if (crateDTO.ManifestType != "Dockyard Plugin Event or Incident Report")
                {
                    errorMsgList.Add("Don't know how to process an EventReport with the Contents: " + crateDTO.Contents);
                    continue;
                }
                var loggingData =_crate.GetContents<LoggingData>(crateDTO);
                currentRouter(loggingData);
            }

            if (errorMsgList.Count > 0)
            {
                throw new InvalidOperationException(String.Join(";;;", errorMsgList));
            }

            return Ok();
            
        }
    }
}