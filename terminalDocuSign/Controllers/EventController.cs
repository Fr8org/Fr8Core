using System.Threading.Tasks;
using System.Web.Http;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using PluginBase.Infrastructure;

namespace terminalDocuSign.Controllers
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
        public async Task<object> ProcessIncomingNotification()
        {
            return await _event.Process(await Request.Content.ReadAsStringAsync());
        }
    }
}
