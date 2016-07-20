using System.Web.Http;
using StructureMap;
using System.Threading.Tasks;
using terminalFr8Core.Interfaces;

namespace terminalFr8Core.Controllers
{
    [RoutePrefix("terminals/terminalFr8Core")]
    public class EventController : ApiController
    {
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
