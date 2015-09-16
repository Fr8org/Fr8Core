using System.Web.Http;
using Core.Interfaces;
using pluginAzureSqlServer.Services;
using StructureMap;

namespace pluginAzureSqlServer.Controllers
{
    public class EventController : ApiController
    {
        private IAzureSqlServerEvent _event;

        public EventController()
        {
            _event = new AzureSqlServerEvent();
        }

        [HttpPost]
        [Route("events")]
        public async void ProcessIncomingNotification()
        {
            _event.Process(await Request.Content.ReadAsStringAsync());
        }
    }
}
