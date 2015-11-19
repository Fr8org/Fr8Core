using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using terminalGoogle.Infrastructure;
using terminalGoogle.Services;

namespace terminalGoogle.Controllers
{
    public class EventController : ApiController
    {
        private IEvent _event;

        public EventController()
        {
            _event = new Event();
        }

        [HttpPost]
        [Route("events")]
        public async void ProcessIncomingNotification()
        {
            _event.Process(await Request.Content.ReadAsStringAsync());
        }
    }
}