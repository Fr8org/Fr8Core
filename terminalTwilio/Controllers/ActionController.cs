using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;

namespace terminalTwilio.Controllers
{    
    [RoutePrefix("actions/terminal_Twilio")]
    public class ActionController : ApiController
    {
        private const string curTerminal = "terminalTwilio";
        private readonly BaseTerminalController _baseTerminalController = new BaseTerminalController();

        [HttpPost]
        public Task<object> Execute([FromUri] String actionType, [FromBody] ActionDTO curActionDTO)
        {
            return _baseTerminalController.HandleFr8Request(curTerminal, actionType, curActionDTO);
        }
    }
}