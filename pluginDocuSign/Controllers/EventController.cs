using System.Threading.Tasks;
using System.Web.Http;
using pluginDocuSign.Services;
using StructureMap;
using pluginDocuSign.Interfaces;

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
        public async Task ProcessIncomingNotification()
        {
            await _event.Process(await Request.Content.ReadAsStringAsync());
        }
    }
}
