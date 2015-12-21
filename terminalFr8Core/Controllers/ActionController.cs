using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Data.Constants;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Managers;
using TerminalBase.BaseClasses;

namespace terminalFr8Core.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curTerminal = "terminalFr8Core";
        private BaseTerminalController _baseTerminalController = new BaseTerminalController();

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