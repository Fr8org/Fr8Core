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
using System.Net;
using System.Net.Http;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using System.Web.Http.Description;
using Fr8.Infrastructure.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers
{
    public class AlarmsController : ApiController
    {
        private static readonly ILog Logger = Fr8.Infrastructure.Utilities.Logging.Logger.GetCurrentClassLogger();

        /// <summary>
        /// Schedules specified alarm to be executed at specified time
        /// </summary>
        /// <remarks>
        /// Start time of the scheduled alarm is definied by its 'start_time' property. <br />
        /// This endpoint is generally designed to allow some activities to delay their execution giving external services time to generate some events or some data
        /// </remarks>
        /// <param name="alarmDTO">Alarm to schedule at specific time</param>
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        [SwaggerResponse(HttpStatusCode.OK, "Alarm was successfully scheduled")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request")]
        [SwaggerResponseRemoveDefaults]
        public async Task<IHttpActionResult> Post(AlarmDTO alarmDTO)
        {
            BackgroundJob.Schedule(() => Execute(alarmDTO), alarmDTO.StartTime);
            return Ok();
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
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
        /// Alarms provide the ability to resend requests with specified data to the terminal until the latter responses with status 200 OK. <br />
        /// It works as follows.A terminal calls this endpoint with specified data and sets the time intervals. <br />
        /// The request has the following form: <br />
        /// <em>/alarms/polling? job_id ={0}&amp;fr8_account_id={1}&amp;minutes={2}&amp;terminal_id={3}</em><br />
        /// So the Hub will iteratively call the terminal until the latter replies with status 200 OK.Each time it will make a POST request with the specified above data in the URL: <br />
        /// <em>[terminalEndpoint]/terminals/[terminalName]/polling?job_id={0}&amp;fr8_account_id={1}&amp;polling_interval={2}</em><br />
        /// <stong>Note:</stong> It should be noted that the terminal is required to have <em>/polling</em> endpoint that accepts data specified above, otherwise the exception will be thrown
        /// </remarks>
        /// <param name="terminalId">Id of the terminal to perform requests to</param>
        /// <param name="pollingData">Parameters of polling requests</param>
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "Polling was successfully initiated")]
        [SwaggerResponseRemoveDefaults]
        public IHttpActionResult Polling([FromUri] string terminalId, [FromBody]PollingDataDTO pollingData)
        {
            Logger.Info($"Polling: requested for {pollingData.ExternalAccountId} from a terminal {terminalId} and addition to jobId {pollingData.AdditionToJobId}");
            pollingData.JobId = terminalId + "|" + pollingData.ExternalAccountId + pollingData.AdditionToJobId;
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
        private static async Task<bool> RenewAuthToken(PollingDataDTO pollingData, string terminalId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminalDO = await ObjectFactory.GetInstance<ITerminal>().GetTerminalByPublicIdentifier(terminalId);
                var token = uow.AuthorizationTokenRepository.FindTokenByExternalAccount(pollingData.ExternalAccountId, terminalDO.Id, pollingData.Fr8AccountId);
                if (token != null)
                {
                    pollingData.AuthToken = token.Token;
                    return true;
                }
            }
            return false;
        }

        private static readonly ILog Logger = Fr8.Infrastructure.Utilities.Logging.Logger.GetCurrentClassLogger();

        public static void ExecuteSchedulledJob(PollingDataDTO pollingData, string terminalId)
        {
            IRestfulServiceClient _client = new RestfulServiceClient();

            //renewing token
            if (!RenewAuthToken(pollingData, terminalId).Result)
            {
                RecurringJob.RemoveIfExists(pollingData.JobId);
                Logger.Info($"Polling: token is missing, removing the job for {pollingData.ExternalAccountId}");
            }

            var request = RequestPolling(pollingData, terminalId, _client);
            var result = request.Result;

            if (result != null)
            {
                if (!result.Result)
                {
                    Logger.Info($"Polling: got result for {pollingData.ExternalAccountId} from a terminal {terminalId}. Deschedulling the job");
                    if (pollingData.RetryCounter > 3)
                    {
                        Logger.Info($"Polling: for {pollingData.ExternalAccountId} from a terminal {terminalId}. Deschedulling the job");
                        RecurringJob.RemoveIfExists(pollingData.JobId);
                    }
                    else
                    {
                        pollingData.RetryCounter++;
                        Logger.Info($"Polling: got result for {pollingData.ExternalAccountId} from a terminal {terminalId}. Starting Retry {pollingData.RetryCounter}");
                        RecurringJob.AddOrUpdate(pollingData.JobId, () => SchedullerHelper.ExecuteSchedulledJob(result, terminalId), "*/" + result.PollingIntervalInMinutes + " * * * *");
                    }
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
                    Logger.Info($"Polling: no result for {pollingData.ExternalAccountId} from a terminal {terminalId}. Last polling was successfull");

                    //in case of ongoing deployment when we have a minimal polling interval, could happen to remove the job. Add default polling interval of 10 minutes in this case as retry
                    pollingData.Result = false;
                    RecurringJob.AddOrUpdate(pollingData.JobId, () => SchedullerHelper.ExecuteSchedulledJob(pollingData, terminalId), "*/" + pollingData.PollingIntervalInMinutes + " * * * *");
                }
                else
                {
                    if (pollingData.RetryCounter > 20)
                    {
                        Logger.Info($"Polling: no result for {pollingData.ExternalAccountId} from a terminal {terminalId}. Remove Job");
                        //last polling was unsuccessfull, so let's deschedulle it
                        RecurringJob.RemoveIfExists(pollingData.JobId);
                    }
                    else
                    {
                        Logger.Info($"Polling: no result for {pollingData.ExternalAccountId} from a terminal {terminalId}. Retry Counter {pollingData.RetryCounter}");
                        pollingData.RetryCounter++;
                        RecurringJob.AddOrUpdate(pollingData.JobId, () => SchedullerHelper.ExecuteSchedulledJob(pollingData, terminalId), "*/" + pollingData.PollingIntervalInMinutes + " * * * *");
                    }

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
                    var url = terminal.Endpoint + "/terminals/" + terminal.Name + "/polling_notifications";
                    Logger.Info($"Polling: executing request for {pollingData?.ExternalAccountId} from {Server.ServerUrl} to a terminal {terminal?.Name} at {terminal?.Endpoint}");

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
                        catch(Exception exception)
                        {
                            Logger.Info($"Polling: problem with terminal polling request for {pollingData?.ExternalAccountId} from {Server.ServerUrl} to a terminal {terminal?.Name}. Exception: {exception.Message}");
                            return null;
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                Logger.Info($"Polling: problem with terminal polling request for {pollingData?.ExternalAccountId} from {Server.ServerUrl} to a terminal. Exception: {exception.Message}");
                return null;
            }
        }
    }
}