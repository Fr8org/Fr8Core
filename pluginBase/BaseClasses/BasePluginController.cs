using System;
using System.Reflection;
using Data.Interfaces.DataTransferObjects;
using System.Web.Http;
using PluginUtilities.Infrastructure;
using System.Threading.Tasks;

namespace PluginBase.BaseClasses
{

    //this is a quasi base class. We can't use inheritance directly because it's across project boundaries, but
    //we can generate instances of this.
    public class BasePluginController : ApiController
    {
        private readonly BasePluginEvent _basePluginEvent;
        public BasePluginController()
        {
            _basePluginEvent = new BasePluginEvent();
        }

        /// <summary>
        /// Reports start up incident
        /// </summary>
        /// <param name="pluginName">Name of the plugin which is starting up</param>
        
        [HttpGet]
        public IHttpActionResult AfterStartup(string pluginName)
        {
            return null;

            //TODO: Commented during development only. So that app loads fast.
            //return Json(ReportStartUp(pluginName));
        }

        /// <summary>
        /// Reports start up event by making a Post request
        /// </summary>
        /// <param name="pluginName"></param>
        private Task<string> ReportStartUp(string pluginName)
        {
            return _basePluginEvent.SendEventOrIncidentReport(pluginName, "Plugin Incident");
        }

        
        /// <summary>
        /// Reports event when process an action
        /// </summary>
        /// <param name="pluginName"></param>
        private Task<string> ReportEvent(string pluginName)
        {
            return _basePluginEvent.SendEventOrIncidentReport(pluginName, "Plugin Event");
        }﻿



        // For /Configure and /Activate actions that accept ActionDTO
        public object HandleDockyardRequest(string curPlugin, string curActionPath, ActionDTO curActionDTO, object dataObject = null)
        {
            if (dataObject == null) dataObject = curActionDTO;

            string curAssemblyName = string.Format("{0}.Actions.{1}_v{2}", curPlugin, curActionDTO.ActivityTemplate.Name, curActionDTO.ActivityTemplate.Version);

            Type calledType = Type.GetType(curAssemblyName + ", " + curPlugin);
            if (calledType == null)
                throw new ArgumentException(string.Format("Action {0}_v{1} doesn't exist in {2} plugin.", 
                    curActionDTO.ActivityTemplate.Name,
                    curActionDTO.ActivityTemplate.Version, 
                    curPlugin), "curActionDTO");
            MethodInfo curMethodInfo = calledType.GetMethod(curActionPath);
            object curObject = Activator.CreateInstance(calledType);
            var response = (object)curMethodInfo.Invoke(curObject, new Object[] { dataObject });
            return response;
        }


    }
}
