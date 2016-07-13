using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using StructureMap;
using terminalStatX.Interfaces;

namespace terminalStatX.Controllers
{
    [RoutePrefix("terminals/terminalStatX")]
    public class EventController : ApiController
    {
        private readonly IHubCommunicator _hubCommunicator;
        private readonly IStatXPolling _polling;

        public EventController(IHubCommunicator hubCommunicator, IStatXPolling polling)
        {
            _hubCommunicator = hubCommunicator;
            _polling = polling;
        }

        [HttpPost]
        [Route("polling_notifications")]
        public async Task<PollingDataDTO> ProcessPollingRequest([FromBody]PollingDataDTO pollingData)
        {
            return await _polling.Poll(_hubCommunicator, pollingData);
        }
    }
}