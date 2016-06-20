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
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Communication;

namespace HubWeb.Controllers
{
    public class AlarmsController : ApiController
    {
        private static readonly ILog Logger = Fr8.Infrastructure.Utilities.Logging.Logger.GetCurrentClassLogger();


        public AlarmsController()
        {
        }

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
        public async Task<IHttpActionResult> Polling([FromUri]string terminalId, [FromBody]PollingDataDTO pollingData)
        {
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
            if (result == null || !result.Result)
                RecurringJob.RemoveIfExists(pollingData.JobId);
            else
                RecurringJob.AddOrUpdate(pollingData.JobId, () => SchedullerHelper.ExecuteSchedulledJob(result, terminalId), "*/" + result.PollingIntervalInMinutes + " * * * *");
        }

        private static async Task<PollingDataDTO> RequestPolling(PollingDataDTO pollingData, string terminalId, IRestfulServiceClient _client)
        {
            try
            {
                var terminalService = ObjectFactory.GetInstance<ITerminal>();

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var terminal = uow.TerminalRepository.GetQuery().Where(a => a.PublicIdentifier == terminalId).FirstOrDefault();
                    string url = terminal.Endpoint + "/terminals/" + terminal.Name + "/polling_notifications";

                    using (var client = new HttpClient())
                    {
                        foreach (var header in terminalService.GetRequestHeaders(terminal))
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }

                        var response = await _client.PostAsync<PollingDataDTO, PollingDataDTO>(new Uri(url), pollingData);

                        return response;
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