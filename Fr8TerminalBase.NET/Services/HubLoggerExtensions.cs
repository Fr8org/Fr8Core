using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities;

namespace Fr8.TerminalBase.Services
{
    public static class HubLoggerExtensions
    {
        public static async Task SendEventOrIncidentReport(this IHubLoggerService loggerService, string eventType)
        {
            await loggerService.Log(new LoggingDataCM
            {
                Data = "service_start_up",
                PrimaryCategory = "Operations",
                SecondaryCategory = "System Startup",
                Activity = "system startup"
            });
        }

        public static async Task SendEventReport(this IHubLoggerService loggerService,  string message)
        {
            //make Post call
            await loggerService.Log(new LoggingDataCM
            {
                Data = message,
                PrimaryCategory = "Operations",
                SecondaryCategory = "System Startup",
                Activity = "system startup"
            });
        }

        /// <summary>
        /// Sends "Terminal Incident" to report terminal Error
        /// </summary>
        /// <param name="exceptionMessage">Exception Message</param>
        /// <param name="exceptionName">Name of the occured exception</param>
        /// <param name="fr8UserId">Id of the current user. It should be obtained from AuthorizationToken</param>
        /// <returns>Response from the fr8 Event Controller</returns>
        public static async Task SendTerminalErrorIncident(this IHubLoggerService loggerService, string exceptionMessage, string exceptionName, string fr8UserId = null)
        {
            await loggerService.Log(new LoggingDataCM
            {
                Fr8UserId = fr8UserId,
                Data = exceptionMessage,
                PrimaryCategory = "TerminalError",
                SecondaryCategory = exceptionName,
                Activity = "Occured"
            });
        }

        public static async Task ReportTerminalError(this IHubLoggerService eventReporter, Exception terminalError, string userId = null)
        {
            var exceptionMessage = terminalError.GetFullExceptionMessage() + "  \n\r   " + terminalError;
            await eventReporter.SendTerminalErrorIncident(exceptionMessage, terminalError.GetType().Name, userId);
        }
    }
}