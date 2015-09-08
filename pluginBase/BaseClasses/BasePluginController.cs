using System;
using System.Configuration;
using System.Reflection;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Entities;
using Newtonsoft.Json;

namespace PluginBase.BaseClasses
{

    //this is a quasi base class. We can't use inheritance directly because it's across project boundaries, but
    //we can generate instances of this.
    public class BasePluginController
    {
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
        public string HandleDockyardRequest(string curPlugin, string curActionPath, ActionDO curActionDO)
        {
            string curAssemblyName = string.Format("pluginAzureSqlServer.Actions.{0}_v{1}", curActionDO.ActionTemplate.Name, curActionDO.ActionTemplate.Version);

            Type calledType = Type.GetType(curAssemblyName);
            MethodInfo curMethodInfo = calledType.GetMethod(curActionPath);
            object curObject = Activator.CreateInstance(calledType);

            return JsonConvert.SerializeObject((object)curMethodInfo.Invoke(curObject, new Object[] { curActionDO }) ?? new { });
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
