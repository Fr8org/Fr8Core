﻿using System;
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
        public async Task<object> Execute([FromUri] String actionType, [FromBody] ActionDTO curActionDTO)
        {
            if (actionType.Equals("run", StringComparison.InvariantCultureIgnoreCase))
                return await (Task<PayloadDTO>)_baseTerminalController.HandleFr8Request(curTerminal, actionType, curActionDTO);
            return await (Task<ActionDTO>)_baseTerminalController.HandleFr8Request(curTerminal, actionType, curActionDTO);
        }
    }
}