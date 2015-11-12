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
<<<<<<< HEAD
            //Implement the processing logic of dockyard core terminal
=======
            //Implement the processing logic of dockyard core plugin
>>>>>>> dev
        }
    }
}
