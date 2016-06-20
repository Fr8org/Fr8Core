using System.Threading.Tasks;
using System.Web.Http;
using Fr8.TerminalBase.Services;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using StructureMap;

namespace terminalGoogle.Controllers
{
    [RoutePrefix("terminals/terminalGoogle")]
    public class EventController : ApiController
    {
        private readonly IHubEventReporter _eventReporter;
        private readonly IEvent _event;
        private readonly IContainer _container;

        public EventController(IHubEventReporter eventReporter, IContainer container)
        {
            _eventReporter = eventReporter;
            _event = new Event();
            _container = container;
        }

        [HttpPost]
        [Route("events")]
        public async Task ProcessIncomingNotification()
        {
            string eventPayLoadContent = await Request.Content.ReadAsStringAsync();

            await _eventReporter.Broadcast(await _event.Process(_container, eventPayLoadContent));
        }
    }
}