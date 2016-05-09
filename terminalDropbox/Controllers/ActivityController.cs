using System;
using System.Diagnostics;
using System.Web.Http;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using TerminalBase.Infrastructure;

namespace terminalDropbox.Controllers
{
    [RoutePrefix("activities")]
    public class ActivityController: BaseTerminalController
    {
        private const string curTerminal = "terminalDropbox";

        [HttpPost]
        [fr8TerminalHMACAuthenticate(curTerminal)]
        [Authorize]
        public Task<object> Execute([FromUri] String actionType, [FromBody] Fr8DataDTO curDataDTO)
        {
            Debug.WriteLine($"Handling request for {actionType} and DTO {curDataDTO}");
            return HandleFr8Request(curTerminal, actionType, curDataDTO);
        }
    }
}