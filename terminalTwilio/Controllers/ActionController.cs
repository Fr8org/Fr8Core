using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;

namespace terminalTwilio.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string cuTerminal = "terminalTwilio";
        private readonly BaseTerminalController _baseTerminalController = new BaseTerminalController();

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>)_baseTerminalController.HandleDockyardRequest(cuTerminal, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_baseTerminalController.HandleDockyardRequest(cuTerminal, "Activate", curActionDTO);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_baseTerminalController.HandleDockyardRequest(cuTerminal, "Deactivate", curActionDTO);
        }
        
        [HttpPost]
        [Route("run")]
        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            return await (Task<PayloadDTO>)_baseTerminalController.HandleDockyardRequest(cuTerminal, "Run", curActionDTO);
        }
    }
}