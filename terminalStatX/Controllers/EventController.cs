using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using StructureMap;
using terminalStatX.Interfaces;

namespace terminalStatX.Controllers
{
    [RoutePrefix("terminals/terminalStatX")]
    public class EventController : ApiController
    {
        private readonly IContainer _container;
        private readonly IEvent _event;
        private readonly IHubEventReporter _reporter;
        private readonly IStatXPolling _polling;

        public EventController(IContainer container, IEvent @event, IHubEventReporter reporter, IStatXPolling polling)
        {
            _container = container;
            _event = @event;
            _reporter = reporter;
            _polling = polling;
        }
        
        [HttpPost]
        [Route("polling_notifications")]
        public async Task<IHttpActionResult> ProcessPollingRequest(string job_id, string fr8_account_id, string polling_interval)
        {
            var hubCommunicator = _container.GetInstance<IHubCommunicator>();

            hubCommunicator.Authorize(fr8_account_id);

            var result = await _polling.Poll(hubCommunicator, job_id, polling_interval);
            if (result)
                return Ok();
            else
                return Content(HttpStatusCode.Gone, "Polling failed, deschedule it");
        }

    }
}