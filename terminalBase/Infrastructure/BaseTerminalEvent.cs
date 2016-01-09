﻿using Data.Crates.Helpers;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using StructureMap;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data.Crates;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Utilities.Configuration.Azure;

namespace TerminalBase.Infrastructure
{
    public class BaseTerminalEvent
    {
        private readonly EventReportCrateFactory _eventReportCrateFactory;
        private readonly LoggingDataCrateFactory _loggingDataCrateFactory;
        private readonly ICrateManager _crateManager;
        
        public delegate Crate EventParser(string externalEventPayload);
        
        private string eventWebServerUrl = string.Empty;
        private bool eventsDisabled = false;

        public BaseTerminalEvent()
        {
            //Missing CoreWebServerUrl most likely means that terminal is running in an integration test environment. 
            //Disable event distribution in such case since we don't need it (nor do we have a Hub instance 
            //to send events too).
            eventWebServerUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl");
            if (string.IsNullOrEmpty(eventWebServerUrl))
            {
                eventsDisabled = true;
            }
            else
            {
                eventWebServerUrl += "api/v1/event/gen1_event";
            }

            _eventReportCrateFactory = new EventReportCrateFactory();
            _loggingDataCrateFactory = new LoggingDataCrateFactory();
            _crateManager = ObjectFactory.GetInstance<CrateManager>();
        }


        public Task<string> SendEventOrIncidentReport(string terminalName, string eventType)
        {
            if (eventsDisabled) return Task.FromResult(string.Empty);

            //SF DEBUG -- Skip this event call for local testing
            //return;


            //make Post call
            var restClient = PrepareRestClient();
            var loggingDataCrate = _loggingDataCrateFactory.Create(new LoggingDataCm
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
            return restClient.PostAsync(new Uri(eventWebServerUrl, UriKind.Absolute),
                _crateManager.ToDto(_eventReportCrateFactory.Create(eventType, terminalName, loggingDataCrate)));

        }

        public Task<string> SendEventReport(string terminalName, string message)
        {
            //SF DEBUG -- Skip this event call for local testing
            //return;

            if (eventsDisabled) return Task.FromResult(string.Empty);

            //make Post call
            var restClient = PrepareRestClient();
            var loggingDataCrate = _loggingDataCrateFactory.Create(new LoggingDataCm
            {
                ObjectId = terminalName,
                CustomerId = "not_applicable",
                Data = message,
                PrimaryCategory = "Operations",
                SecondaryCategory = "System Startup",
                Activity = "system startup"
            });
            //TODO inpect this
            //I am not sure what to supply for parameters eventName and palletId, so i passed terminalName and eventType
            return restClient.PostAsync(new Uri(eventWebServerUrl, UriKind.Absolute),
                _crateManager.ToDto(_eventReportCrateFactory.Create("Terminal Event", terminalName, loggingDataCrate)));

        }

        /// <summary>
        /// Sends "Terminal Incident" to report terminal Error
        /// </summary>
        /// <param name="terminalName">Name of the terminal where the exception occured</param>
        /// <param name="exceptionMessage">Exception Message</param>
        /// <param name="exceptionName">Name of the occured exception</param>
        /// <returns>Response from the fr8 Event Controller</returns>
        public Task<string> SendTerminalErrorIncident(string terminalName, string exceptionMessage, string exceptionName)
        {
            if (eventsDisabled) return Task.FromResult(string.Empty);

            //prepare the REST client to make the POST to fr8's Event Controller
            var restClient = PrepareRestClient();

            //create event logging data with required information
            var loggingDataCrate = _loggingDataCrateFactory.Create(new LoggingDataCm
            {
                ObjectId = terminalName,
                CustomerId = "",
                Data = exceptionMessage,
                PrimaryCategory = "TerminalError",
                SecondaryCategory = exceptionName,
                Activity = "Occured"
            });

            //return the response from the fr8's Event Controller
            return restClient.PostAsync(new Uri(eventWebServerUrl, UriKind.Absolute),
                _crateManager.ToDto(_eventReportCrateFactory.Create("Terminal Incident", terminalName, loggingDataCrate)));
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

        /// <summary>
        /// Processing the external event pay load received
        /// </summary>
        /// <param name="curExternalEventPayload">event pay load received</param>
        /// <param name="parser">delegate method</param>
        public async Task Process(string curExternalEventPayload,EventParser parser)
        {
            var fr8EventUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl") + "api/v1/event/processevents";
            var eventReportCrateDTO = _crateManager.ToDto(parser.Invoke(curExternalEventPayload));
            
            if (eventReportCrateDTO != null)
            {
                Uri url = new Uri(fr8EventUrl, UriKind.Absolute);
                try
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = new TimeSpan(0, 10, 0); //10 minutes
                    await client.PostAsJsonAsync(url, eventReportCrateDTO);
                }
                catch (TaskCanceledException)
                {
                    //Timeout
                    throw new TimeoutException(
                        String.Format("Timeout while making HTTP request.  \r\nURL: {0},   \r\nMethod: {1}",
                        url.ToString(),
                        HttpMethod.Post.Method));
                }
            }
        }
    }
}