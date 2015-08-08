using System;
using System.Web.Http;
using Core.Interfaces;
using Data.Entities;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers
{
    /// <summary>
    /// Central logging Events controller
    /// </summary>
    public class EventController : ApiController
    {
        private readonly IEventService _eventService;

        public EventController()
        {
            _eventService = ObjectFactory.GetInstance<IEventService>();
        }

        [HttpPost]
        public IHttpActionResult Event(EventDTO submittedEvent)
        {
            if (submittedEvent.EventType.Equals("Plugin Incident"))
            {
                var incidentDo = new IncidentDO
                {
                    ObjectId = submittedEvent.Data.ObjectId,
                    CustomerId = submittedEvent.Data.CustomerId,
                    Data = submittedEvent.Data.Data,
                    PrimaryCategory = submittedEvent.Data.PrimaryCategory,
                    SecondaryCategory = submittedEvent.Data.SecondaryCategory,
                    Activity = submittedEvent.Data.Activity
                };

                if (_eventService.HandlePluginIncident(incidentDo))
                {
                    return Ok();
                }

                return
                    InternalServerError(new Exception("Updating incident is failed due to internal server error."));
            }

            return BadRequest("Only plugin in incidennt is handled by this method.");
        }
    }
}