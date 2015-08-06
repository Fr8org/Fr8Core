using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.Interfaces;
using Newtonsoft.Json.Linq;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers
{
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
                _eventService.HandlePluginIncident();
            }
            return Ok();
        }

        //// GET api/<controller>
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<controller>/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<controller>
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/<controller>/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/<controller>/5
        //public void Delete(int id)
        //{
        //}
    }
}