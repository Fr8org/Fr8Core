using System.Threading.Tasks;
using System.Web.Http;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Controllers
{
    public class EventController : ApiController
    {
        private IEvent _event;
        private BaseTerminalEvent _baseTerminalEvent;

        public EventController()
        {
            _event = new Event();
            _baseTerminalEvent = new BaseTerminalEvent();
        }

        [HttpPost]
        [Route("events")]
        public async Task<object> ProcessIncomingNotification()
        {
            return await _event.Process(await Request.Content.ReadAsStringAsync());
        }
    }
}
