using System.Threading.Tasks;
using System.Web.Http;
using Fr8.TerminalBase.Services;
using terminalSlack.Interfaces;
using terminalSlack.Services;

namespace terminalSlack.Controllers
{
    [RoutePrefix("terminals/terminalSlack")]
    public class EventController : ApiController
    {
        private readonly IHubEventReporter _eventReporter;
        private readonly IEvent _event;

        public EventController(IHubEventReporter eventReporter)
        {
            _eventReporter = eventReporter;
            _event = new Event();
        }

        [HttpPost]
        [Route("events")]
        public async Task ProcessIncomingNotification()
        {
            //_event.Process(await Request.Content.ReadAsStringAsync());
            string eventPayLoadContent = await Request.Content.ReadAsStringAsync();
            await _eventReporter.Broadcast(await _event.Process(eventPayLoadContent));
        }
    }
}
