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
        private readonly IEvent _eventService;

        public EventController()
        {
            _eventService = ObjectFactory.GetInstance<IEvent>();
        }

        [HttpPost]
        public IHttpActionResult Event(EventDTO submittedEvent)
        {
            if (submittedEvent.EventType.Equals("Plugin Incident"))
            {
                _eventService.HandlePluginIncident(submittedEvent.Data);
                return Ok();
            }

            throw new InvalidOperationException("Only plugin incidents are handled at this moment.");
        }
    }
}