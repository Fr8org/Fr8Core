using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Crates.Helpers;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtilities.Infrastructure
{
    public class BasePluginEvent
    {
        private readonly EventReportCrateFactory _eventReportCrateFactoryHelper;
        private readonly LoggingDataCrateFactory _loggingDataCrateFactoryHelper;
        public BasePluginEvent()
        {
            _eventReportCrateFactoryHelper = new EventReportCrateFactory();
            _loggingDataCrateFactoryHelper = new LoggingDataCrateFactory();
        }


        public Task<string> SendEventOrIncidentReport(string pluginName, string eventType)
        {
            //SF DEBUG -- Skip this event call for local testing
            //return;


            //make Post call
            var restClient = PrepareRestClient();
            const string eventWebServerUrl = "EventWebServerUrl";
            string url = ConfigurationManager.AppSettings[eventWebServerUrl];
            var loggingDataCrate = _loggingDataCrateFactoryHelper.Create(new LoggingData
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
                _eventReportCrateFactoryHelper.Create(eventType, pluginName, loggingDataCrate));

        }

        /// <summary>
        /// Initializes a new rest call
        /// </summary>
        private IRestfulServiceClient PrepareRestClient()
        {
            var restCall = new RestfulServiceClient();
            return restCall;
        }
    }
}
