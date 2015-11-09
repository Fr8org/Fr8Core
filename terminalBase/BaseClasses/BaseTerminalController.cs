using System;
using System.Reflection;
using Data.Interfaces.DataTransferObjects;
using System.Web.Http;
using TerminalBase.Infrastructure;
using System.Threading.Tasks;
using Utilities.Configuration.Azure;

namespace TerminalBase.BaseClasses
{

    //this is a quasi base class. We can't use inheritance directly because it's across project boundaries, but
    //we can generate instances of this.
    public class BaseTerminalController : ApiController
    {
        private readonly BaseTerminalEvent _baseTerminalEvent;
        public BaseTerminalController()
        {
            _baseTerminalEvent = new BaseTerminalEvent();
        }

        /// <summary>
        /// Reports Plugin Error incident
        /// </summary>
        [HttpGet]
        public IHttpActionResult ReportTerminalError(string terminalName, Exception terminalError)
        {
            var exceptionMessage = string.Format("{0}\r\n{1}", terminalError.Message, terminalError.StackTrace);
            return Json(_baseTerminalEvent.SendTerminalErrorIncident(terminalName, exceptionMessage, terminalError.GetType().Name));
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
            return _baseTerminalEvent.SendEventOrIncidentReport(pluginName, "Plugin Incident");
        }

        
        /// <summary>
        /// Reports event when process an action
        /// </summary>
        /// <param name="pluginName"></param>
        private Task<string> ReportEvent(string pluginName)
        {
            return _baseTerminalEvent.SendEventOrIncidentReport(pluginName, "Plugin Event");
        }

        // For /Configure and /Activate actions that accept ActionDTO
        public object HandleDockyardRequest(string curPlugin, string curActionPath, ActionDTO curActionDTO, object dataObject = null)
        {
            if (curActionDTO == null)
                throw new ArgumentNullException("curActionDTO");
            if (curActionDTO.ActivityTemplate == null)
                throw new ArgumentException("ActivityTemplate is null", "curActionDTO");
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
