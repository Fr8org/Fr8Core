using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;
using System.Threading.Tasks;

namespace pluginTwilio.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginTwilio";
        private readonly BasePluginController _basePluginController = new BasePluginController();

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>)_basePluginController.HandleDockyardRequest(curPlugin, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Activate", curActionDTO);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Deactivate", curActionDTO);
        }
        
        [HttpPost]
        [Route("execute")]
        public async Task<PayloadDTO> Execute(ActionDTO curActionDTO)
        {
            return await (Task<PayloadDTO>)_basePluginController.HandleDockyardRequest(curPlugin, "Execute", curActionDTO);
        }
    }
}