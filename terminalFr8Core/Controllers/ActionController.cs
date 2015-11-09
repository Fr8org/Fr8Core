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
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>) _baseTerminalController
                .HandleDockyardRequest(curTerminal, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_baseTerminalController.HandleDockyardRequest(curTerminal, "Activate", curActionDTO);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_baseTerminalController.HandleDockyardRequest(curTerminal, "Deactivate", curActionDTO);
        }

        [HttpPost]
        [Route("run")]
        public async Task<PayloadDTO> Run(ActionDTO actionDto)
        {
            return await (Task<PayloadDTO>) _baseTerminalController.HandleDockyardRequest(
                curTerminal, "Run", actionDto);
        }
    }
}