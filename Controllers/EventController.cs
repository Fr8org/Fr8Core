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

        private delegate void EventHandler(LoggingData loggingData);

        public EventController()
        {
            _event = ObjectFactory.GetInstance<IEvent>();
        }

        [HttpPost]
        public IHttpActionResult Post(CrateDTO submittedEvents)
        {
            var serializer = new JsonSerializer();
            var eventDTO = serializer.Deserialize<EventDTO>(submittedEvents.Contents);

            //Request of alex to keep things simple for now
            if (eventDTO.CrateStorage.Count != 1)
            {
                throw new InvalidOperationException("Only single crate can be processed for now.");
            }

            EventHandler currentHandler;

            if (eventDTO.EventName.Equals("Plugin Incident"))
            {
                currentHandler = _event.HandlePluginIncident;
            }
            else if (eventDTO.EventName.Equals("Plugin Event"))
            {
                currentHandler = _event.HandlePluginEvent;
            }
            else
            {
                throw new InvalidOperationException("Unknown EventDTO with name: " + eventDTO.EventName);
            }


            var errorMsgList = new List<string>();
            foreach (var crateDTO in eventDTO.CrateStorage)
            {
                if (crateDTO.ManifestType != "Dockyard Plugin Event or Incident Report")
                {
                    errorMsgList.Add("Don't know how to process an EventReport with the Contents: " + crateDTO.Contents);
                    continue;
                }

                var loggingData = serializer.Deserialize<LoggingData>(crateDTO.Contents);
                currentHandler(loggingData);
            }

            if (errorMsgList.Count > 0)
            {
                throw new InvalidOperationException(String.Join(";;;", errorMsgList));
            }

            return Ok();
            
        }
    }
}