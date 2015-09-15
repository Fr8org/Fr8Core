using System;
using System.Configuration;
using System.Reflection;
using Core.Managers.APIManagers.Transmitters.Restful;
using Core.Services;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Crates.Helpers;
using Newtonsoft.Json;
using Data.Interfaces.DataTransferObjects;

namespace PluginBase.BaseClasses
{

    //this is a quasi base class. We can't use inheritance directly because it's across project boundaries, but
    //we can generate instances of this.
    public class BasePluginController
    {
        private readonly EventReportCrate _eventReportCrateHelper;
        private readonly LoggingDataCrate _loggingDataCrateHelper;
        public BasePluginController()
        {
            _eventReportCrateHelper = new EventReportCrate();
            _loggingDataCrateHelper = new LoggingDataCrate();
        }

        /// <summary>
        /// Reports start up incident
        /// </summary>
        /// <param name="pluginName">Name of the plugin which is starting up</param>
        public  void AfterStartup(string pluginName)
        {
            ReportStartUp(pluginName);
        }

        /// <summary>
        /// Reports start up event by making a Post request
        /// </summary>
        /// <param name="pluginName"></param>
        private  void ReportStartUp(string pluginName)
        {
            SendEventOrIncidentReport(pluginName,  "Plugin Incident");
        }

        
        /// <summary>
        /// Reports event when process an action
        /// </summary>
        /// <param name="pluginName"></param>
        private  void ReportEvent(string pluginName)
        {
            SendEventOrIncidentReport(pluginName, "Plugin Event");
        }﻿

        private  void SendEventOrIncidentReport(string pluginName, string eventType)
        {
            //SF DEBUG -- Skip this event call for local testing
            //return;


            //make Post call
            var restClient = PrepareRestClient();
            const string eventWebServerUrl = "EventWebServerUrl";
            string url = ConfigurationManager.AppSettings[eventWebServerUrl];
            var loggingDataCrate = _loggingDataCrateHelper.Create(new LoggingData
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
            restClient.PostAsync(new Uri(url, UriKind.Absolute),
                _eventReportCrateHelper.Create(eventType, pluginName, loggingDataCrate)).Wait();

        }

        // For /Configure and /Activate actions that accept ActionDTO
        public string HandleDockyardRequest(string curPlugin, string curActionPath, ActionDTO curActionDTO, object dataObject = null)
        {
            if (dataObject == null) dataObject = curActionDTO;

            string curAssemblyName = string.Format("{0}.Actions.{1}_v{2}", curPlugin, curActionDTO.ActivityTemplate.Name, curActionDTO.ActivityTemplate.Version);

            Type calledType = Type.GetType(curAssemblyName + ", " + curPlugin);
            MethodInfo curMethodInfo = calledType.GetMethod(curActionPath);
            object curObject = Activator.CreateInstance(calledType);
            var response = (object) curMethodInfo.Invoke(curObject, new Object[] {dataObject});

            return JsonConvert.SerializeObject( response ?? new { });
        }

        /// <summary>
        /// Initializes a new rest call
        /// </summary>
        private  IRestfulServiceClient PrepareRestClient()
        {
            var restCall = new RestfulServiceClient();
            return restCall;
        }
    }
}
