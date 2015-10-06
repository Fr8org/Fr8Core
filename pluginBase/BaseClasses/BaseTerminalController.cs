using System;
using System.Reflection;
using Data.Interfaces.DataTransferObjects;
using System.Web.Http;
using TerminalBase.Infrastructure;
using System.Threading.Tasks;

namespace terminalBase.BaseClasses
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
        /// Reports Terminal Error incident
        /// </summary>
        [HttpGet]
        public IHttpActionResult ReportTerminalError(string terminalName, Exception terminalError)
        {
            return Json(_baseTerminalEvent.SendTerminalErrorIncident(terminalName, terminalError.Message, terminalError.GetType().Name));
        }

        /// <summary>
        /// Reports start up incident
        /// </summary>
        /// <param name="terminalName">Name of the terminal which is starting up</param>
        [HttpGet]
        public IHttpActionResult AfterStartup(string terminalName)
        {
            return null;

            //TODO: Commented during development only. So that app loads fast.
            //return Json(ReportStartUp(terminalName));
        }

        /// <summary>
        /// Reports start up event by making a Post request
        /// </summary>
        /// <param name="terminalName"></param>
        private Task<string> ReportStartUp(string terminalName)
        {
            return _baseTerminalEvent.SendEventOrIncidentReport(terminalName, "Terminal Incident");
        }

        
        /// <summary>
        /// Reports event when process an action
        /// </summary>
        /// <param name="terminalName"></param>
        private Task<string> ReportEvent(string terminalName)
        {
            return _baseTerminalEvent.SendEventOrIncidentReport(terminalName, "Terminal Event");
        }

        // For /Configure and /Activate actions that accept ActionDTO
        public object HandleDockyardRequest(string curTerminal, string curActionPath, ActionDTO curActionDTO, object dataObject = null)
        {
            if (curActionDTO == null)
                throw new ArgumentNullException("curActionDTO");
            if (curActionDTO.ActivityTemplate == null)
                throw new ArgumentException("ActivityTemplate is null", "curActionDTO");
            if (dataObject == null) dataObject = curActionDTO;

            string curAssemblyName = string.Format("{0}.Actions.{1}_v{2}", curTerminal, curActionDTO.ActivityTemplate.Name, curActionDTO.ActivityTemplate.Version);

            Type calledType = Type.GetType(curAssemblyName + ", " + curTerminal);
            if (calledType == null)
                throw new ArgumentException(string.Format("Action {0}_v{1} doesn't exist in {2} terminal.", 
                    curActionDTO.ActivityTemplate.Name,
                    curActionDTO.ActivityTemplate.Version,
                    curTerminal), "curActionDTO");
            MethodInfo curMethodInfo = calledType.GetMethod(curActionPath);
            object curObject = Activator.CreateInstance(calledType);
            var response = (object)curMethodInfo.Invoke(curObject, new Object[] { dataObject });
            return response;
        }
    }
}
