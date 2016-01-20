using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;
using System;
using TerminalBase.Infrastructure;

namespace terminalAtlassian.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController: BaseTerminalController
    {
        private const string curTerminal = "terminalAtlassian";

        [HttpPost]
        [fr8TerminalHMACAuthenticate(curTerminal)]
        [Authorize]
        public Task<object> Execute([FromUri] String actionType, [FromBody] ActionDTO curActionDTO)
        {
            return HandleFr8Request(curTerminal, actionType, curActionDTO);
        }
    }
}