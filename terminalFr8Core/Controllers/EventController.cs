using System.Web.Http;
using StructureMap;

namespace terminalFr8Core.Controllers
{
    public class EventController : ApiController
    {
        [HttpPost]
        [Route("events")]
        public void ProcessIncomingNotification()
        {
            //Implement the processing logic of dockyard core terminal

        }
    }
}
