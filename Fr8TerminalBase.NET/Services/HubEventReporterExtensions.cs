using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates.Helpers;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities;

namespace Fr8.TerminalBase.Services
{
    public static class HubEventReporterExtensions
    {
        public static async Task SendEventOrIncidentReport(this IHubEventReporter eventReporter, string eventType)
        {
            var loggingDataCrate = LoggingDataCrateFactory.Create(new LoggingDataCM
            {
                ObjectId = eventReporter.Terminal.Name,
                Data = "service_start_up",
                PrimaryCategory = "Operations",
                SecondaryCategory = "System Startup",
                Activity = "system startup"
            });

            var master = await eventReporter.GetMasterHubCommunicator();
            await master.SendEvent(loggingDataCrate);
        }

        public static async Task SendEventReport(this IHubEventReporter eventReporter,  string message)
        {
            //make Post call
            var loggingDataCrate = LoggingDataCrateFactory.Create(new LoggingDataCM
            {
                ObjectId = eventReporter.Terminal.Name,
                Data = message,
                PrimaryCategory = "Operations",
                SecondaryCategory = "System Startup",
                Activity = "system startup"
            });

            var master = await eventReporter.GetMasterHubCommunicator();
            await master.SendEvent(loggingDataCrate);
        }

        /// <summary>
        /// Sends "Terminal Incident" to report terminal Error
        /// </summary>
        /// <param name="terminalName">Name of the terminal where the exception occured</param>
        /// <param name="exceptionMessage">Exception Message</param>
        /// <param name="exceptionName">Name of the occured exception</param>
        /// <param name="fr8UserId">Id of the current user. It should be obtained from AuthorizationToken</param>
        /// <returns>Response from the fr8 Event Controller</returns>
        public static async Task SendTerminalErrorIncident(this IHubEventReporter eventReporter, string exceptionMessage, string exceptionName, string fr8UserId = null)
        {
            //create event logging data with required information
            var loggingDataCrate = LoggingDataCrateFactory.Create(new LoggingDataCM
            {
                Fr8UserId = fr8UserId,
                ObjectId = eventReporter.Terminal.Name,
                Data = exceptionMessage,
                PrimaryCategory = "TerminalError",
                SecondaryCategory = exceptionName,
                Activity = "Occured"
            });

            var master = await eventReporter.GetMasterHubCommunicator();
            await master.SendEvent(loggingDataCrate);
        }

        public static async Task ReportTerminalError(this IHubEventReporter eventReporter, Exception terminalError, string userId = null)
        {
            var exceptionMessage = terminalError.GetFullExceptionMessage() + "  \n\r   " + terminalError;
            await eventReporter.SendTerminalErrorIncident(exceptionMessage, terminalError.GetType().Name, userId);
        }
    }
}