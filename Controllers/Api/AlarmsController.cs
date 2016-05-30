using Hangfire;
using Hub.Infrastructure;
using Hub.Interfaces;
using StructureMap;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8Data.DataTransferObjects;
using HubWeb.Infrastructure_HubWeb;
using log4net;
using Data.Interfaces;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Validation.Providers;

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
        public async Task Polling(string job_id, string fr8_account_id, string minutes, string terminal_id)
        {
            string jobId = job_id.GetHashCode().ToString();
            RecurringJob.AddOrUpdate(jobId, () => ExecuteSchedulledJob(job_id, fr8_account_id, minutes, terminal_id), "*/" + minutes + " * * * *");
        }

        private void ExecuteSchedulledJob(string job_id, string fr8AccountId, string minutes, string terminal_id)
        {
            var request = RequestPolling(job_id, fr8AccountId, minutes, terminal_id);
            var result = request.Result;
            if (!result)
                RecurringJob.RemoveIfExists(job_id.GetHashCode().ToString());
        }

        private async Task<bool> RequestPolling(string job_id, string fr8_account_id, string minutes, string terminal_id)
        {
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var terminal = uow.TerminalRepository.GetQuery().Where(a => a.PublicIdentifier == terminal_id).FirstOrDefault();
                    string url = terminal.Endpoint + "/terminals/" + terminal.Name + "/polling_notifications?"
                        + string.Format("job_id={0}&fr8_account_id={1}&polling_interval={2}", job_id, fr8_account_id, minutes);

                    using (var client = new HttpClient())
                    {
                        var response = await client.PostAsync(url, null);
                        return response.StatusCode == System.Net.HttpStatusCode.OK;
                    }

                }
            }
            catch
            {
                return false;
            }
        }
    }
}