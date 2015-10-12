using System.Web.Http;
using StructureMap;
using pluginDocuSign.Interfaces;
using pluginDocuSign.Services;
using PluginUtilities.Infrastructure;

namespace pluginDocuSign.Controllers
{
    public class EventController : ApiController
    {
        private IEvent _event;
        private BasePluginEvent _basePluginEvent;

        public EventController()
        {
            _event = new Event();
            _basePluginEvent = new BasePluginEvent();
        }

        [HttpPost]
        [Route("events")]
        public async void ProcessIncomingNotification()
        {
            PluginUtilities.Infrastructure.BasePluginEvent.EventParser parser = new BasePluginEvent.EventParser(_event.ProcessEvent);
            string eventPayLoadContent = Request.Content.ReadAsStringAsync().Result;
            await _basePluginEvent.Process(eventPayLoadContent, parser);
        }
    }
}
