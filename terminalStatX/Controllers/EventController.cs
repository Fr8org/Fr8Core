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
        private readonly IContainer _container;
        private readonly IStatXPolling _polling;

        public EventController(IContainer container, IStatXPolling polling)
        {
            _container = container;
            _polling = polling;
        }

        [HttpPost]
        [Route("polling_notifications")]
        public async Task<PollingDataDTO> ProcessPollingRequest([FromBody]PollingDataDTO pollingData)
        {
            var hubCommunicator = _container.GetInstance<IHubCommunicator>();

            hubCommunicator.Authorize(pollingData.Fr8AccountId);

            pollingData = await _polling.Poll(hubCommunicator, pollingData);

            return pollingData;
        }
    }
}