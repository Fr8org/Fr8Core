using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Data.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Data.Entities;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalSlack.Actions;
using terminalSlack.Interfaces;
using terminalSlack.Services;

namespace terminalSlack.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : BaseTerminalController
    {
        private const string curTerminal = "terminalSlack";
        private readonly BaseTerminalController _baseTerminalController;

        [HttpPost]
        [fr8TerminalHMACAuthenticate(curTerminal)]
        [Authorize]
        public Task<object> Execute([FromUri] String actionType, [FromBody] ActivityDTO curActionDTO)
        {
            return HandleFr8Request(curTerminal, actionType, curActionDTO);
        }
    }
}