using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hangfire;
using Hub.Interfaces;
using Hub.Managers;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace HubWeb.Controllers
{
    [RoutePrefix("api/alarm")]
    public class AlarmController : ApiController
    {
        [Route("notify")]
        [HttpPost]
        public async Task<IHttpActionResult> Notify(AlarmDTO alarmDTO)
        {
            var startTime = DateTimeOffset.Parse(alarmDTO.startTime);
            var containerId = Guid.Parse(alarmDTO.containerId);
            var terminalName = alarmDTO.terminalName;
            var terminalVersion = alarmDTO.terminalVersion;

            return await Notify(startTime, containerId, terminalName, terminalVersion);
        }
        
        public async Task<IHttpActionResult> Notify(DateTimeOffset startTime, Guid containerId, string terminalName, string terminalVersion)
        {
            Expression<Action> action = () => VerifyContainer(startTime, containerId, terminalName, terminalVersion);
            BackgroundJob.Schedule(action, startTime);

            var eventController = new EventController();
            return await eventController.ProcessIncomingEvents(terminalName, terminalVersion);
        }
        
        public async void VerifyContainer(DateTimeOffset startTime, Guid containerId, string terminalName, string terminalVersion)
        {
            HttpResponseMessage result = null;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var container = uow.ContainerRepository.GetByKey(containerId);
                if (container != null && container.ContainerState == ContainerState.Pending)
                {
                    var crateManager = ObjectFactory.GetInstance<ICrateManager>();

                    var label = String.Format("Alarm Triggered [{0}]", startTime.ToUniversalTime());
                    var logItemList = new List<LogItemDTO>();

                    crateManager.AddLogMessage(label, logItemList, container);
                    var terminal = ObjectFactory.GetInstance<ITerminal>();
                    var terminalUrl = terminal.ParseTerminalUrlFor(terminalName, terminalVersion, "action/run");

                    result = await new HttpClient().PostAsync(new Uri(terminalUrl, UriKind.Absolute), Request.Content);
                }
            }
        }
    }
}
