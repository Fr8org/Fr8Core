using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;

namespace terminalPapertrail.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curTerminal = "terminalPapertrail";
        private BaseTerminalController _baseTerminalController = new BaseTerminalController();


        [HttpPost]
        public async Task<ActionDTO> Execute([FromUri] String actionType, [FromBody] ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>)_baseTerminalController.HandleFr8Request(curTerminal, actionType, curActionDTO);
        }
    }
}