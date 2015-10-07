using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Crates.Helpers;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace TerminalBase.Infrastructure
{
    public class BaseTerminalEvent
    {
        private readonly EventReportCrateFactory _eventReportCrateFactory;
        private readonly LoggingDataCrateFactory _loggingDataCrateFactory;

        public BaseTerminalEvent()
        {
            _eventReportCrateFactory = new EventReportCrateFactory();
            _loggingDataCrateFactory = new LoggingDataCrateFactory();
        }


        public Task<string> SendEventOrIncidentReport(string terminalName, string eventType)
        {
            //SF DEBUG -- Skip this event call for local testing
            //return;


            //make Post call
            var restClient = PrepareRestClient();
            const string eventWebServerUrl = "EventWebServerUrl";
            string url = ConfigurationManager.AppSettings[eventWebServerUrl];
            var loggingDataCrate = _loggingDataCrateFactory.Create(new LoggingData
            {
                ObjectId = terminalName,
                CustomerId = "not_applicable",
                Data = "service_start_up",
                PrimaryCategory = "Operations",
                SecondaryCategory = "System Startup",
                Activity = "system startup"
            });
            //TODO inpect this
            //I am not sure what to supply for parameters eventName and palletId, so i passed terminalName and eventType
            return restClient.PostAsync(new Uri(url, UriKind.Absolute),
                _eventReportCrateFactory.Create(eventType, terminalName, loggingDataCrate));

        }

        /// <summary>
        /// Sends "Terminal Incident" to report Terminal Error
        /// </summary>
        /// <param name="terminalName">Name of the terminal where the exception occured</param>
        /// <param name="exceptionMessage">Exception Message</param>
        /// <param name="exceptionName">Name of the occured exception</param>
        /// <returns>Response from the fr8 Event Controller</returns>
        public Task<string> SendTerminalErrorIncident(string terminalName, string exceptionMessage, string exceptionName)
        {
            //prepare the REST client to make the POST to fr8's Event Controller
            var restClient = PrepareRestClient();
            const string eventWebServerUrl = "EventWebServerUrl";
            string url = ConfigurationManager.AppSettings[eventWebServerUrl];

            //create event logging data with required information
            var loggingDataCrate = _loggingDataCrateFactory.Create(new LoggingData
            {
                ObjectId = terminalName,
                CustomerId = "",
                Data = exceptionMessage,
                PrimaryCategory = "TerminalError",
                SecondaryCategory = exceptionName,
                Activity = "Occured"
            });

            //return the response from the fr8's Event Controller
            return restClient.PostAsync(new Uri(url, UriKind.Absolute),
                _eventReportCrateFactory.Create("Terminal Incident", terminalName, loggingDataCrate));
        }

        /// <summary>
        /// Initializes a new rest call
        /// </summary>
        /// <returns>The protected access specifier is only for Unit Test purpose. 
        /// In all other scenarios it should be teated as private</returns>
        protected virtual IRestfulServiceClient PrepareRestClient()
        {
            var restCall = new RestfulServiceClient();
            return restCall;
        }
    }
}