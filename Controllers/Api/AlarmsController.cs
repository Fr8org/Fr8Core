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

namespace HubWeb.Controllers
{
    public class AlarmsController : ApiController
    {
        private static readonly ILog Logger = Fr8.Infrastructure.Utilities.Logging.Logger.GetCurrentClassLogger();

        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Post(AlarmDTO alarmDTO)
        {
            BackgroundJob.Schedule(() => Execute(alarmDTO), alarmDTO.StartTime);

            //TODO: Commented as part of DO - 1520. Need to rethink about this.
            //var eventController = new EventController();
            //return await eventController.ProcessIncomingEvents(alarmDTO.TerminalName, alarmDTO.TerminalVersion);
            return Ok();
        }

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

        [HttpPost]
        public async Task<IHttpActionResult> Polling([FromUri] string terminalId, [FromBody]PollingDataDTO pollingData)
        {
            pollingData.JobId = terminalId + "|" + pollingData.ExternalAccountId;
            RecurringJob.AddOrUpdate(pollingData.JobId, () => SchedullerHelper.ExecuteSchedulledJob(pollingData, terminalId), "*/" + pollingData.PollingIntervalInMinutes + " * * * *");
            if (pollingData.TriggerImmediately)
                RecurringJob.Trigger(pollingData.JobId);

            return Ok();
        }
    }


    public static class SchedullerHelper
    {
        public static void ExecuteSchedulledJob(PollingDataDTO pollingData, string terminalId)
        {
            IRestfulServiceClient _client = new RestfulServiceClient();
            var request = RequestPolling(pollingData, terminalId, _client);
            var result = request.Result;

            if (result != null)
            {
                if (!result.Result)
                    RecurringJob.RemoveIfExists(pollingData.JobId);
                else
                    RecurringJob.AddOrUpdate(pollingData.JobId, () => SchedullerHelper.ExecuteSchedulledJob(result, terminalId), "*/" + result.PollingIntervalInMinutes + " * * * *");
            }
            else
            {
                //we didn't get any response from the terminal (it might have not started yet, for example) Let's give it one more chance, and if it will fail - the job will be descheduled cause of Result set to false;
                pollingData.Result = false;
                RecurringJob.AddOrUpdate(pollingData.JobId, () => SchedullerHelper.ExecuteSchedulledJob(result, terminalId), "*/" + result.PollingIntervalInMinutes + " * * * *");
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