using System;
﻿using System.Threading.Tasks;
using System.Web.Http;
using Fr8Data.DataTransferObjects;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalTwilio.Controllers
{    
    [RoutePrefix("activities")]
    public class ActivityController : BaseTerminalController
    {
        private const string curTerminal = "terminalTwilio";

        [HttpPost]
        [fr8TerminalHMACAuthenticate(curTerminal)]
        [Authorize]
        public Task<object> Execute([FromUri] String actionType, [FromBody] Fr8DataDTO curDataDTO)
        {
            return HandleFr8Request(curTerminal, actionType, curDataDTO);
        }
    }
}