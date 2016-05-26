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
                var plan = ObjectFactory.GetInstance<IPlan>();
                var continueTask = plan.Continue(alarmDTO.ContainerId);
                Task.WaitAll(continueTask);
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

        public void ExecuteSchedulledJob(string external_account_id, string fr8AccountId, string minutes, string terminalId)
        {
            var request = RequestPolling(external_account_id, fr8AccountId, minutes, terminalId);
            var result = request.Result;
            if (!result)
                RecurringJob.RemoveIfExists(external_account_id.GetHashCode().ToString());
        }

        public async Task<bool> RequestPolling(string external_account_id, string fr8AccountId, string minutes, string terminalId)
        {
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var terminal = uow.TerminalRepository.GetQuery().Where(a => a.PublicIdentifier == terminalId).FirstOrDefault();
                    string url = terminal.Endpoint + "/terminals/" + terminal.Name + "/polling_notifications?"
                        + string.Format("job_id={0}&fr8_account_id={1}&polling_interval={2}", external_account_id, fr8AccountId, minutes);

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