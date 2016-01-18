using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Data.Entities;
using TerminalBase.BaseClasses;
using System.Collections.Generic;
using Data.States;
using System;
using System.Threading.Tasks;
using AutoMapper;
using TerminalBase.Infrastructure;

namespace terminalExcel.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : BaseTerminalController
    {
        private const string curTerminal = "terminalExcel";

        [HttpPost]
        [fr8TerminalHMACAuthorize(curTerminal)]
        public Task<object> Execute([FromUri] String actionType, [FromBody] ActionDTO curActionDTO)
        {
            return HandleFr8Request(curTerminal, actionType, curActionDTO);
        }
    }
}