using System;
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
            var eventCrateList = serializer.DeserializeList<CrateDTO>(submittedEvents.Contents);

            //This??????????
            if (eventCrateList.Any(crateDTO => crateDTO.ManifestType != "Dockyard Plugin Event or Incident Report"))
            {
                throw new InvalidOperationException("Unknown crate in request");
            }

            //or this??????????????????


            //Make sure there are no invalid crates before starting processing operation
            foreach (var crateDTO in eventCrateList.Where(crateDTO => crateDTO.ManifestType != "Dockyard Plugin Event or Incident Report"))
            {
                throw new InvalidOperationException("Don't know how to process an EventReport with the Contents: " + crateDTO.Contents);
            }

            foreach (var crateDTO in eventCrateList)
            {
                var loggingData = serializer.Deserialize<LoggingData>(crateDTO.Contents);
                _event.HandlePluginIncident(loggingData);
            }
            return Ok();
        }
    }
}