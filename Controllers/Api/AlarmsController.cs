using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hangfire;
using Hub.Interfaces;
using Hub.Managers;
using HubWeb.Infrastructure;
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
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Post(AlarmDTO alarmDTO)
        {
            //TODO what happens to AlarmsController? does it stay in memory all this time?
            //TODO inspect this and change callback function to a static function if necessary
            Expression<Action> action = () => ExecuteTerminalWithLogging(alarmDTO);
#if DEBUG
            BackgroundJob.Schedule(action, DateTime.Now.AddSeconds(10));
#else
            BackgroundJob.Schedule(action, alarmDTO.StartTime);
#endif

            //TODO: Commented as part of DO - 1520. Need to rethink about this.
            //var eventController = new EventController();
            //return await eventController.ProcessIncomingEvents(alarmDTO.TerminalName, alarmDTO.TerminalVersion);
            return Ok();
        }

        //TODO is this method called from somewhere else?
        [HttpPost]
        public async void ExecuteTerminalWithLogging(AlarmDTO alarmDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var _plan = ObjectFactory.GetInstance<IPlan>();
                await _plan.Continue(alarmDTO.ContainerId);
                //TODO report output to somewhere to pusher service maybe

                /*
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

                    
                }
                 * */
            }
        }
    }
}