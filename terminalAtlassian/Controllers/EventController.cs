using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using StructureMap;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using terminalAtlassian.Interfaces;
using terminalAtlassian.Models;

namespace terminalAtlassian.Controllers
{
    [RoutePrefix("terminals/terminalAtlassian")]
    public class EventController : ApiController
    {
        private readonly IHubEventReporter _eventReporter;
        private readonly IContainer _container;
        private readonly IAtlassianEventManager _event;

        public EventController(IHubEventReporter eventReporter, IContainer container, IAtlassianEventManager eventManager)
        {
            _eventReporter = eventReporter;
            _container = container;
            _event = eventManager;
        }

        [HttpPost]
        [Route("process-issue")]
        public async Task<IHttpActionResult> ProcessIncomingIssue()
        {
            var eventPayLoadContent = await Request.Content.ReadAsStringAsync();
            await _eventReporter.Broadcast(await _event.ProcessExternalEvents(eventPayLoadContent));
            return Ok();
        }

        [HttpPost]
        [Route("events")]
        public async Task<IHttpActionResult> ProcessHubEvents()
        {
            var eventPayLoadContent = await Request.Content.ReadAsStringAsync();
            await _event.ProcessInternalEvents(_container, eventPayLoadContent);
            return Ok();
        }
    }
}