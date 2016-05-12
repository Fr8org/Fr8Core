using System;
using System.Web.Http;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using TerminalBase.Infrastructure;

namespace terminalBox.Controllers
{
    [RoutePrefix("activities")]
    public class ActivityController: BaseTerminalController
    {
        private const string CurTerminal = "terminalBox";

        [HttpPost]
        [fr8TerminalHMACAuthenticate(CurTerminal)]
        [Authorize]
        public Task<object> Execute([FromUri] String actionType, [FromBody] Fr8DataDTO curDataDTO)
        {
            return HandleFr8Request(CurTerminal, actionType, curDataDTO);
        }


    }
}