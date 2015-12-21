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
using terminalSlack.Actions;
using terminalSlack.Interfaces;
using terminalSlack.Services;

namespace terminalSlack.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curTerminal = "terminalSlack";
        private readonly BaseTerminalController _baseTerminalController;

        public ActionController()
        {
            _baseTerminalController = new BaseTerminalController();
        }

        [HttpPost]
        public Task<object> Execute([FromUri] String type, [FromUri] string state, [FromBody] ActionDTO curActionDTO)
        {
            ActionState? actionState = null;
            if (!string.IsNullOrEmpty(state))
            {
                actionState = (ActionState)Enum.Parse(typeof(ActionState), state, true);
            }
            return _baseTerminalController.HandleFr8Request(curTerminal, actionState, type, curActionDTO);
        }
    }
}