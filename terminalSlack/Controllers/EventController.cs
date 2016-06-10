using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Infrastructure;
using terminalSlack.Interfaces;
using terminalSlack.Services;

namespace terminalSlack.Controllers
{
    [RoutePrefix("terminals/terminalSlack")]
    public class EventController : ApiController
    {
        private readonly IEvent _event;
        private readonly BaseTerminalEvent _baseTerminalEvent;

        public EventController(IRestfulServiceClient restfulServiceClient)
        {
            _event = new Event();
            _baseTerminalEvent = new BaseTerminalEvent(restfulServiceClient);
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
