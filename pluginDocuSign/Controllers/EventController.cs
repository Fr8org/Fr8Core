using System.Web.Http;
using pluginDocuSign.Services;

namespace pluginDocuSign.Controllers
{
    public class EventController : ApiController
    {
        private IDocuSignEvent _event;

        public EventController()
        {
            _event = new DocuSignEvent();
        }

        [HttpPost]
        [Route("events")]
        public async void ProcessIncomingNotification()
        {
            _event.Process(await Request.Content.ReadAsStringAsync());
        }
    }
}
