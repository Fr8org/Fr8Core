using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;
using System;

namespace terminalAtlassian.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController:ApiController
    {
        private const string curTerminal = "terminalAtlassian";
        private BaseTerminalController _baseTerminalController = new BaseTerminalController();

        [HttpPost]
        public Task<object> Execute([FromUri] String actionType, [FromBody] ActionDTO curActionDTO)
        {
            return _baseTerminalController.HandleFr8Request(curTerminal, actionType, curActionDTO);
        }
    }
}