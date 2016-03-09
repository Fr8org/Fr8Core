using System.Web.Http;
using StructureMap;
using TerminalBase.Infrastructure;
using System.Threading.Tasks;
using terminalFr8Core.Interfaces;

namespace terminalFr8Core.Controllers
{
    [RoutePrefix("terminals/terminalFr8Core")]
    public class EventController : ApiController
    {

        private IEvent _event;
        private BaseTerminalEvent _baseTerminalEvent;

        public EventController()
        {
            _baseTerminalEvent = new BaseTerminalEvent();
        }

        [HttpPost]
        [Route("events")]
        public async Task<IHttpActionResult> ProcessIncomingNotification()
        {
            //Implement the processing logic of dockyard core terminal
            //string eventPayLoadContent = await Request.Content.ReadAsStringAsync();
            //await _baseTerminalEvent.Process(eventPayLoadContent, _event.Process);
            return Ok("Processed Fr8 events notification successfully.");
        }
    }
}
