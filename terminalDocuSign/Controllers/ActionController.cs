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
            ActionDO submittedActionDO = Mapper.Map<ActionDO>(curActionDTO);

            var resultActionDO = await (Task<ActionDO>)_basePluginController.HandleDockyardRequest(curPlugin, "Configure", submittedActionDO);

            var resultActionDTO = Mapper.Map<ActionDTO>(resultActionDO);

            return resultActionDTO;
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDataPackage)
        {
            ActionDO submittedActionDO = Mapper.Map<ActionDO>(curActionDataPackage);

            var resultActionDO = _basePluginController.HandleDockyardRequest(curPlugin, "Activate", submittedActionDO);

            var resultActionDTO = Mapper.Map<ActionDTO>(resultActionDO);

            return resultActionDTO;
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDataPackage)
        {
            ActionDO submittedActionDO = Mapper.Map<ActionDO>(curActionDataPackage);

            var resultActionDO = _basePluginController.HandleDockyardRequest(curPlugin, "Deactivate", submittedActionDO);

            var resultActionDTO = Mapper.Map<ActionDTO>(resultActionDO);

            return resultActionDTO;
        }

        [HttpPost]
        [Route("run")]
        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            ActionDO submittedActionDO = Mapper.Map<ActionDO>(curActionDTO);

            var resultPayloadDTO = await (Task<PayloadDTO>)_basePluginController.HandleDockyardRequest(curPlugin, "Run", submittedActionDO);

            return resultPayloadDTO;

        }
    }
}