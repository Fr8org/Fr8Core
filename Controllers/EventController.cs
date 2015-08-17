using System;
using System.Web.Http;
using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;

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
        public IHttpActionResult Event(EventDTO submittedEvent)
        {
            if (submittedEvent.EventType.Equals("Plugin Incident"))
            {
                _event.HandlePluginIncident(submittedEvent.Data);
                return Ok();
            }
            else if (submittedEvent.EventType.Equals("Plugin Event"))
            {
                _event.HandlePluginEvent(submittedEvent.Data);
                return Ok();
            }

            throw new InvalidOperationException("Plugin incident and event are handled.");
        }
    }
}