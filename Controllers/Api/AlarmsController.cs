using Data.Interfaces.DataTransferObjects;
using Hangfire;
using Hub.Infrastructure;
using Hub.Interfaces;
using StructureMap;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using Utilities.Configuration;

namespace HubWeb.Controllers
{
    public class AlarmsController : ApiController
    {
        private static readonly ILog Logger = Utilities.Logging.Logger.GetCurrentClassLogger();

        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Post(AlarmDTO alarmDTO)
        {
            //TODO what happens to AlarmsController? does it stay in memory all this time?
            //TODO inspect this and change callback function to a static function if necessary

            //put Hubs job in "hub" queue to avoid processing of terminalDocuSign jobs

            BackgroundJob.Schedule(() => Execute(alarmDTO), alarmDTO.StartTime);

            //TODO: Commented as part of DO - 1520. Need to rethink about this.
            //var eventController = new EventController();
            //return await eventController.ProcessIncomingEvents(alarmDTO.TerminalName, alarmDTO.TerminalVersion);
            return Ok();
        }

        [Queue("hub"), MoveToTheHubQueueAttribute]
        public void Execute(AlarmDTO alarmDTO)
        {
            try
            {
                var plan = ObjectFactory.GetInstance<IPlan>();
                var continueTask = plan.Continue(alarmDTO.ContainerId);
                Task.WaitAll(continueTask);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to run container: {alarmDTO.ContainerId}", ex);
            }

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

        //TODO is this method called from somewhere else?
        [HttpPost]
        // as for now it seems that this is the only way to specify queue when you are schedulig the job
        //https://discuss.hangfire.io/t/how-schedule-a-delayed-job-to-a-specific-queue/911
        [Queue("hub")]
        public void ExecuteTerminalWithLogging(AlarmDTO alarmDTO)
        {
            Execute(alarmDTO);
        }
    }
}