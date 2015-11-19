using System;
using System.Globalization;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;

namespace terminalTwilio.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curTerminal = "terminalTwilio";
        private readonly BaseTerminalController _baseTerminalController = new BaseTerminalController();

        [HttpPost]
        public async Task<ActionDTO> Execute([FromUri] String actionType, [FromBody] ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>)_baseTerminalController.HandleFr8Request(curTerminal, CultureInfo.CurrentCulture.TextInfo.ToTitleCase(actionType), curActionDTO);
        }
    }
}