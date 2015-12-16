using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hangfire;
using Hub.Interfaces;
using Hub.Managers;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;

namespace HubWeb.Controllers
{
    public class AlarmsController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> Post(AlarmDTO alarmDTO)
        {
            Expression<Action> action = () => ExecuteTerminalWithLogging(alarmDTO);
            BackgroundJob.Schedule(action, alarmDTO.StartTime);

            //TODO: Commented as part of DO - 1520. Need to rethink about this.
            //var eventController = new EventController();
            //return await eventController.ProcessIncomingEvents(alarmDTO.TerminalName, alarmDTO.TerminalVersion);
            return null;
        }

        [HttpPost]
        public async void ExecuteTerminalWithLogging(AlarmDTO alarmDTO)
        {
            HttpResponseMessage result = null;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var container = uow.ContainerRepository.GetByKey(alarmDTO.ContainerId);
                if (container != null && container.ContainerState == ContainerState.Pending)
                {
                    var crateManager = ObjectFactory.GetInstance<ICrateManager>();

                    var label = String.Format("Alarm Triggered [{0}]", alarmDTO.StartTime.ToUniversalTime());
                    var logItemList = new List<LogItemDTO>();

                    crateManager.AddLogMessage(label, logItemList, container);
                    var terminal = ObjectFactory.GetInstance<ITerminal>();
                    var terminalUrl = terminal.ParseTerminalUrlFor(alarmDTO.TerminalName, alarmDTO.TerminalVersion, "action/run");
                    var content = new ObjectContent<ActionDTO>(alarmDTO.ActionDTO, new JsonMediaTypeFormatter());

                    result = await new HttpClient().PostAsync(new Uri(terminalUrl, UriKind.Absolute), content);
                }
            }
        }
    }
}