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
        private const string curPlugin = "terminalSendGrid";
        private BasePluginController _basePluginController = new BasePluginController();

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>)_basePluginController.HandleDockyardRequest(curPlugin, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("run")]
        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            return await (Task<PayloadDTO>)_basePluginController.HandleDockyardRequest(curPlugin, "Run", curActionDTO);
        }
    }
}