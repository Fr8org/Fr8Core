using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using PluginBase.BaseClasses;
using System.Threading.Tasks;

namespace pluginSendGrid.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginSendGrid";
        private BasePluginController _basePluginController = new BasePluginController();

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>)_basePluginController.HandleDockyardRequest(curPlugin, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("execute")]
        public async Task<PayloadDTO> Execute(ActionDTO curActionDTO)
        {
            return await (Task<PayloadDTO>)_basePluginController.HandleDockyardRequest(curPlugin, "Execute", curActionDTO);
        }
    }
}