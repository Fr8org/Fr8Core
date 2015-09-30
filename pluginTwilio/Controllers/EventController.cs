using System.Web.Http;

namespace pluginTwilio.Controllers
{
    public class EventController : ApiController
    {
        [HttpPost]
        [Route("events")]
        public void ProcessIncomingNotification()
        {
        }
    }
}
