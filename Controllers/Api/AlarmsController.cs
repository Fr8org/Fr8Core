using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
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
using Utilities.Interfaces;

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

            //put Hubs job in "hub" queue to avoid processing of terminalDocuSign jobs

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
        // as for now it seems that this is the only way to specify queue when you are schedulig the job
        //https://discuss.hangfire.io/t/how-schedule-a-delayed-job-to-a-specific-queue/911
        [Queue("hub")]
        public void ExecuteTerminalWithLogging(AlarmDTO alarmDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var _plan = ObjectFactory.GetInstance<IPlan>();
                var continueTask = _plan.Continue(alarmDTO.ContainerId);
                Task.WaitAll(continueTask);
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
                    var terminalUrl = terminal.ParseTerminalUrlFor(alarmDTO.TerminalName, alarmDTO.TerminalVersion, "activity/run");
                    var content = new ObjectContent<ActionDTO>(alarmDTO.ActionDTO, new JsonMediaTypeFormatter());

                    
                }
                 * */
            }
        }
    }
}