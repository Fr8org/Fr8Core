using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;

namespace terminalTwilio.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "terminalTwilio";
        private readonly BasePluginController _basePluginController = new BasePluginController();

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>)_basePluginController.HandleFr8Request(curPlugin, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleFr8Request(curPlugin, "Activate", curActionDTO);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleFr8Request(curPlugin, "Deactivate", curActionDTO);
        }
        
        [HttpPost]
        [Route("run")]
        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            return await (Task<PayloadDTO>)_basePluginController.HandleFr8Request(curPlugin, "Run", curActionDTO);
        }
    }
}