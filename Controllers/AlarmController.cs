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
    [RoutePrefix("api/alarms")]
    public class AlarmController : ApiController
    {
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> SetAlarm(AlarmDTO alarmDTO)
        {
            DateTimeOffset startTime;
            if (!DateTimeOffset.TryParse(alarmDTO.StartTime, out startTime))
            {
                return ResponseMessage(
                    Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ArgumentException("\'StartTime\' has invalid format", "startTime")));
            }

            Guid containerId = alarmDTO.ContainerId;
            string terminalName = alarmDTO.TerminalName;
            string terminalVersion = alarmDTO.TerminalVersion;
            
            return await SetAlarm(startTime, containerId, terminalName, terminalVersion);
        }
        
        public async Task<IHttpActionResult> SetAlarm(DateTimeOffset startTime, Guid containerId, string terminalName, string terminalVersion)
        {
            Expression<Action> action = () => ExecuteTerminalWithLogging(startTime, containerId, terminalName, terminalVersion);
            BackgroundJob.Schedule(action, startTime);

            var eventController = new EventController();
            return await eventController.ProcessIncomingEvents(terminalName, terminalVersion);
        }
        
        public async void ExecuteTerminalWithLogging(DateTimeOffset startTime, Guid containerId, string terminalName, string terminalVersion)
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
