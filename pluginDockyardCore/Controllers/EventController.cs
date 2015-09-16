using System.Web.Http;
using Core.Interfaces;
using pluginDockyardCore.Services;
using StructureMap;

namespace pluginDockyardCore.Controllers
{
    public class EventController : ApiController
    {
        private IDockyardCoreEvent _event;
        public EventController()
        {
            _event = new DockyardCoreEvent();
        }

        [HttpPost]
        [Route("events")]
        public async void ProcessIncomingNotification()
        {
            _event.Process(await Request.Content.ReadAsStringAsync());
        }
    }
}
