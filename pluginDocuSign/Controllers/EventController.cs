using System.Web.Http;
using StructureMap;
using pluginDocuSign.Interfaces;
using pluginDocuSign.Services;

namespace pluginDocuSign.Controllers
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
