using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;

namespace terminalSendGrid.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curTerminal = "terminalSendGrid";
        private BaseTerminalController _baseTerminalController = new BaseTerminalController();

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>)_baseTerminalController.HandleDockyardRequest(curTerminal, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("run")]
        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            return await (Task<PayloadDTO>)_baseTerminalController.HandleDockyardRequest(curTerminal, "Run", curActionDTO);
        }
    }
}