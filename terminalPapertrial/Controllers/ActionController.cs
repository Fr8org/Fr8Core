using System.Threading.Tasks;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;

namespace terminalPapertrial.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curTerminal = "terminalPapertrial";
        private BaseTerminalController _baseTerminalController = new BaseTerminalController();


        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>)_baseTerminalController.HandleFr8Request(curTerminal, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDTO)
        {

            return (ActionDTO)_baseTerminalController.HandleFr8Request(curTerminal, "Activate", curActionDTO);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_baseTerminalController.HandleFr8Request(curTerminal, "Deactivate", curActionDTO);
        }

        [HttpPost]
        [Route("run")]
        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            return await (Task<PayloadDTO>)_baseTerminalController.HandleFr8Request(curTerminal, "Run", curActionDTO);

        }
    }
}