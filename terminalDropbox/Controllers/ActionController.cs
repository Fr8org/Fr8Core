using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;

namespace terminalDropbox.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController:ApiController
    {
        private const string curTerminal = "terminalDropbox";
        private BaseTerminalController _baseTerminalController = new BaseTerminalController();

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>)_baseTerminalController
              .HandleFr8Request(curTerminal, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDataPackage)
        {
            return (ActionDTO)_baseTerminalController.HandleFr8Request(curTerminal, "Activate", curActionDataPackage);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDataPackage)
        {
            return (ActionDTO)_baseTerminalController.HandleFr8Request(curTerminal, "Deactivate", curActionDataPackage);
        }

        [HttpPost]
        [Route("run")]
        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            return await (Task<PayloadDTO>)_baseTerminalController.HandleFr8Request(curTerminal, "Run", curActionDTO);
        }
    }
}