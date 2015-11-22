using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
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
        public async Task<object> Execute([FromUri] String actionType, [FromBody] ActionDTO curActionDTO)
        {
            if (actionType.Equals("run", StringComparison.InvariantCultureIgnoreCase))
                return await (Task<PayloadDTO>)_baseTerminalController.HandleFr8Request(curTerminal, actionType, curActionDTO);
            return await (Task<ActionDTO>)_baseTerminalController.HandleFr8Request(curTerminal, actionType, curActionDTO);
        }
    }
}