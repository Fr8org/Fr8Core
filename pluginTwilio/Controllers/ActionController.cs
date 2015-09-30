using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;

namespace pluginTwilio.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private readonly BasePluginController _basePluginController = new BasePluginController();

        [HttpPost]
        [Route("configure")]
        public ActionDTO Configure(ActionDTO curActionDto)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(Settings.PluginName, "Configure", curActionDto);
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDto)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(Settings.PluginName, "Activate", curActionDto);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDto)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(Settings.PluginName, "Deactivate", curActionDto);
        }
        
        [HttpPost]
        [Route("execute")]
        public ActionDTO Execute(ActionDataPackageDTO curActionDataPackage)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(Settings.PluginName, "Execute", curActionDataPackage.ActionDTO, curActionDataPackage);
        }
    }
}