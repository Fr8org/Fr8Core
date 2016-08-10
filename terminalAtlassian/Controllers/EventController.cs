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

        public EventController(IHubEventReporter eventReporter, IContainer container)
        {
            _eventReporter = eventReporter;
            _container = container;
        }

        [HttpPost]
        [Route("subscribe")]
        public async Task<IHttpActionResult> ProcessIncomingMedia()
        {
            var content = await Request.Content.ReadAsStringAsync();
            var issue = JsonConvert.DeserializeObject<JiraIssueEvent>(content);
            return Ok();
        }
    }
}