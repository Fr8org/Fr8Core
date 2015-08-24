
using System;
using System.Configuration;
using Core.Managers.APIManagers.Transmitters.Restful;
using Utilities;

namespace PluginUtilities
{
    public static class PluginBase
    {
        /// <summary>
        /// Reports start up incident
        /// </summary>
        /// <param name="pluginName">Name of the plugin which is starting up</param>
        public static void AfterStartup(string pluginName)
        {
            ReportStartUp(pluginName);
        }

        /// <summary>
        /// Reports start up event by making a Post request
        /// </summary>
        /// <param name="pluginName"></param>
        private static void ReportStartUp(string pluginName)
        {
            //SendEventOrIncidentReport(pluginName,  "Plugin Incident");
        }

        
        /// <summary>
        /// Reports event when process an action
        /// </summary>
        /// <param name="pluginName"></param>
        private static void ReportEvent(string pluginName)
        {
            SendEventOrIncidentReport(pluginName, "Plugin Event");
        }﻿

        private static void SendEventOrIncidentReport(string pluginName, string eventType)
        {
            //SF DEBUG -- Skip this event call for local testing
            //return;


            //make Post call
            var restClient = PrepareRestClient();
            const string eventWebServerUrl = "EventWebServerUrl";
            string url = ConfigurationManager.AppSettings[eventWebServerUrl];
            restClient.PostAsync(new Uri(url, UriKind.Absolute),
                new
                {
                    Source = pluginName,
                    EventType = eventType,
                    Data = new
                    {
                        ObjectId = pluginName,
                        CustomerId = "not_applicable",
                        Data = "service_start_up",
                        PrimaryCategory = "Operations",
                        SecondaryCategory = "System Startup",
                        Activity = "system startup",
                    }
                }).Wait();

        }

        /// <summary>
        /// Initializes a new rest call
        /// </summary>
        private static IRestfulServiceClient PrepareRestClient()
        {
            var restCall = new RestfulServiceClient();
            return restCall;
        }
    }
}
