using System.Threading.Tasks;
using System.Web.Http;
using StructureMap;
using TerminalBase.Infrastructure;
using terminalSlack.Interfaces;
using terminalSlack.Services;

namespace terminalSlack.Controllers
{
    [RoutePrefix("terminals/terminalSlack")]
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
        public async Task ProcessIncomingNotification()
        {
            //_event.Process(await Request.Content.ReadAsStringAsync());
            string eventPayLoadContent = await Request.Content.ReadAsStringAsync();
            await _baseTerminalEvent.Process(eventPayLoadContent, _event.Process);
        }
    }
}
