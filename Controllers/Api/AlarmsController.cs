using Hangfire;
using Hub.Infrastructure;
using Hub.Interfaces;
using StructureMap;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using HubWeb.Infrastructure_HubWeb;
using log4net;
using Data.Interfaces;
using System.Linq;
using System.Net.Http;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using System.Web.Http.Description;

namespace HubWeb.Controllers
{
    public class AlarmsController : ApiController
    {
        private static readonly ILog Logger = Fr8.Infrastructure.Utilities.Logging.Logger.GetCurrentClassLogger();

        /// <summary>
        /// Schedules specified alarm to be executed at specified time
        /// </summary>
        /// <param name="alarmDTO">Alarm to schedule at its startTime property</param>
        /// <response code="200">Alarm was succesfully scheduled</response>
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Post(AlarmDTO alarmDTO)
        {
            BackgroundJob.Schedule(() => Execute(alarmDTO), alarmDTO.StartTime);
            return Ok();
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public void Execute(AlarmDTO alarmDTO)
        {
            try
            {
                var containerService = ObjectFactory.GetInstance<IContainerService>();
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var container = uow.ContainerRepository.GetByKey(alarmDTO.ContainerId);
                    if (container == null)
                    {
                        throw new Exception($"Container {alarmDTO.ContainerId} was not found.");
                    }

                    var continueTask = containerService.Continue(uow, container);
                    Task.WaitAll(continueTask);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to run container: {alarmDTO.ContainerId}", ex);
            }

            //TODO report output to somewhere to pusher service maybe
        }
        /// <summary>
        /// Initiates periodic requests to the terminal with the specified Id configured with specified settings
        /// </summary>
        /// <remarks>
        /// Alarms provide the ability to resend requests with specified data to the terminal until the latter responses with status 200 OK. It works as follows. A terminal calls this endpoint with specified data and sets the time intervals. The Hub then will iteratively call the terminal until the latter replies with status 200 OK
        /// </remarks>
        /// <param name="terminalId">Id of the terminal to perform requests to</param>
        /// <param name="pollingData">Parameters of polling requests</param>
        /// <response code="200">Polling was successfully initiated</response>
        [HttpPost]
        public IHttpActionResult Polling([FromUri] string terminalId, [FromBody]PollingDataDTO pollingData)
        {
            Logger.Info($"Polling: requested for {pollingData.ExternalAccountId} from a terminal {terminalId}");
            pollingData.JobId = terminalId + "|" + pollingData.ExternalAccountId;
            RecurringJob.AddOrUpdate(pollingData.JobId, () => SchedullerHelper.ExecuteSchedulledJob(pollingData, terminalId), "*/" + pollingData.PollingIntervalInMinutes + " * * * *");
            if (pollingData.TriggerImmediately)
            {
                RecurringJob.Trigger(pollingData.JobId);
            }
            return Ok();
        }
    }

    public static class SchedullerHelper
    {
        private static readonly ILog Logger = Fr8.Infrastructure.Utilities.Logging.Logger.GetCurrentClassLogger();

        public static void ExecuteSchedulledJob(PollingDataDTO pollingData, string terminalId)
        {
            Logger.Info($"Polling: executing request for {pollingData.ExternalAccountId} to a terminal {terminalId}");
            IRestfulServiceClient _client = new RestfulServiceClient();
            var request = RequestPolling(pollingData, terminalId, _client);
            var result = request.Result;

            if (result != null)
            {
                if (!result.Result)
                {
                    Logger.Info($"Polling: got result for {pollingData.ExternalAccountId} from a terminal {terminalId}. Deschedulling the job");
                    RecurringJob.RemoveIfExists(pollingData.JobId);
                }
                else
                {
                    Logger.Info($"Polling: got result for {pollingData.ExternalAccountId} from a terminal {terminalId}. Success");
                    RecurringJob.AddOrUpdate(pollingData.JobId, () => SchedullerHelper.ExecuteSchedulledJob(result, terminalId), "*/" + result.PollingIntervalInMinutes + " * * * *");
                }
            }
            else
            {
                Logger.Info($"Polling: no result for {pollingData.ExternalAccountId} from a terminal {terminalId}. Terminal didn't answer");
                //we didn't get any response from the terminal (it might have not started yet, for example) Let's give it one more chance, and if it will fail - the job will be descheduled cause of Result set to false;
                if (pollingData.Result) //was the job successfull last time we polled?
                {
                    pollingData.Result = false;
                    RecurringJob.AddOrUpdate(pollingData.JobId, () => SchedullerHelper.ExecuteSchedulledJob(pollingData, terminalId), "*/" + pollingData.PollingIntervalInMinutes + " * * * *");
                }
                else
                {
                    //last polling was unsuccessfull, so let's deschedulle it
                    RecurringJob.RemoveIfExists(pollingData.JobId);
                }
            }
        }

        private static async Task<PollingDataDTO> RequestPolling(PollingDataDTO pollingData, string terminalId, IRestfulServiceClient _client)
        {
            try
            {
                var terminalService = ObjectFactory.GetInstance<ITerminal>();

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var terminal = uow.TerminalRepository.GetQuery().FirstOrDefault(a => a.PublicIdentifier == terminalId);
                    string url = terminal.Endpoint + "/terminals/" + terminal.Name + "/polling_notifications";

                    using (var client = new HttpClient())
                    {
                        foreach (var header in terminalService.GetRequestHeaders(terminal))
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }

                        try
                        {
                            var response = await _client.PostAsync<PollingDataDTO, PollingDataDTO>(new Uri(url), pollingData);

                            return response;
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}