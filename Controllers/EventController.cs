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

            var errorMsgList = new List<string>();
            foreach (var crateDTO in eventDTO.CrateStorage)
            {
                if (crateDTO.ManifestType != "Dockyard Plugin Event or Incident Report")
                {
                    errorMsgList.Add("Don't know how to process an EventReport with the Contents: " + crateDTO.Contents);
                    continue;
                }

                var loggingData = serializer.Deserialize<LoggingData>(crateDTO.Contents);
                _event.HandlePluginEvent(loggingData);
            }

            if (errorMsgList.Count > 0)
            {
                throw new InvalidOperationException(String.Join(";;;", errorMsgList));
            }

            return Ok();
            
        }
    }
}