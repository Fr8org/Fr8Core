using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Data.Entities;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalYammer.Actions;
using terminalYammer.Interfaces;
using terminalYammer.Services;

namespace terminalYammer.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : BaseTerminalController
    {
        private const string curTerminal = "terminalYammer";

        [HttpPost]
        [fr8TerminalHMACAuthenticate(curTerminal)]
        [Authorize]
        public Task<object> Execute([FromUri] String actionType, [FromBody] Fr8DataDTO curDataDTO)
        {
            return HandleFr8Request(curTerminal, actionType, curDataDTO);
        }
    }
}