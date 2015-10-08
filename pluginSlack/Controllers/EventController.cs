using System.Web.Http;
using Core.Interfaces;
using StructureMap;

namespace pluginSlack.Controllers
{
    public class EventController : ApiController
    {
        [HttpPost]
        [Route("events")]
        public void ProcessIncomingNotification()
        {
            //implement the processing logic for slack plugin external events
        }
    }
}
