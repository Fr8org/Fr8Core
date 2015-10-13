using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Data.Entities;
using PluginBase.BaseClasses;
using System.Collections.Generic;
using Data.States;
using System;
using System.Threading.Tasks;

namespace pluginExcel.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginExcel";
        private BasePluginController _basePluginController = new BasePluginController();

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>) _basePluginController.HandleDockyardRequest(curPlugin, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Activate", curActionDTO);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Deactivate", curActionDTO);
        }

        [HttpPost]
        [Route("execute")]
        public async Task<ActionDTO> Execute(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>) _basePluginController.HandleDockyardRequest(curPlugin, "Execute", curActionDTO);
        }
    }
}