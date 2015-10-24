using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using terminalDocuSign.DataTransferObjects;
using Utilities.Configuration.Azure;

namespace terminalDocuSign.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "terminalDocuSign";
        private BasePluginController _basePluginController = new BasePluginController();


        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>) _basePluginController
                .HandleDockyardRequest(curPlugin, "Configure", curActionDTO);
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
        [Route("run")]
        public async Task<PayloadDTO> Run(ActionDTO actionDto)
        {
            return await (Task<PayloadDTO>)_basePluginController.HandleDockyardRequest(
                curPlugin, "Run", actionDto);
        }
    }
}