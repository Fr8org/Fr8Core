using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;
using TerminalBase.Infrastructure;

namespace terminalSendGrid.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : BaseTerminalController
    {
        private const string curTerminal = "terminalSendGrid";

        [HttpPost]
        [fr8TerminalHMACAuthorize(curTerminal)]
        public Task<object> Execute([FromUri] String actionType, [FromBody] ActionDTO curActionDTO)
        {
            return HandleFr8Request(curTerminal, actionType, curActionDTO);
        }
    }
}