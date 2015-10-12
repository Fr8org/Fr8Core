using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Crates.Helpers;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Configuration;
using System.Threading.Tasks;
using fr8.Microsoft.Azure;

namespace PluginUtilities.Infrastructure
{
    public class BasePluginEvent
    {
        private readonly EventReportCrateFactory _eventReportCrateFactory;
        private readonly LoggingDataCrateFactory _loggingDataCrateFactory;

        public BasePluginEvent()
        {
            _eventReportCrateFactory = new EventReportCrateFactory();
            _loggingDataCrateFactory = new LoggingDataCrateFactory();
        }


        public Task<string> SendEventOrIncidentReport(string pluginName, string eventType)
        {
            //SF DEBUG -- Skip this event call for local testing
            //return;


            //make Post call
            var restClient = PrepareRestClient();
            const string eventWebServerUrl = "EventWebServerUrl";
            string url = CloudConfigurationManager.GetSetting(eventWebServerUrl);
            var loggingDataCrate = _loggingDataCrateFactory.Create(new LoggingData
            {
                ObjectId = pluginName,
                CustomerId = "not_applicable",
                Data = "service_start_up",
                PrimaryCategory = "Operations",
                SecondaryCategory = "System Startup",
                Activity = "system startup"
            });
            //TODO inpect this
            //I am not sure what to supply for parameters eventName and palletId, so i passed pluginName and eventType
            return restClient.PostAsync(new Uri(url, UriKind.Absolute),
                _eventReportCrateFactory.Create(eventType, pluginName, loggingDataCrate));

        }

        /// <summary>
        /// Sends "Plugin Incident" to report Plugin Error
        /// </summary>
        /// <param name="pluginName">Name of the plugin where the exception occured</param>
        /// <param name="exceptionMessage">Exception Message</param>
        /// <param name="exceptionName">Name of the occured exception</param>
        /// <returns>Response from the fr8 Event Controller</returns>
        public Task<string> SendPluginErrorIncident(string pluginName, string exceptionMessage, string exceptionName)
        {
            //prepare the REST client to make the POST to fr8's Event Controller
            var restClient = PrepareRestClient();
            const string eventWebServerUrl = "EventWebServerUrl";
            string url = CloudConfigurationManager.GetSetting(eventWebServerUrl);

            //create event logging data with required information
            var loggingDataCrate = _loggingDataCrateFactory.Create(new LoggingData
            {
                ObjectId = pluginName,
                CustomerId = "",
                Data = exceptionMessage,
                PrimaryCategory = "PluginError",
                SecondaryCategory = exceptionName,
                Activity = "Occured"
            });

            //return the response from the fr8's Event Controller
            return restClient.PostAsync(new Uri(url, UriKind.Absolute),
                _eventReportCrateFactory.Create("Plugin Incident", pluginName, loggingDataCrate));
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