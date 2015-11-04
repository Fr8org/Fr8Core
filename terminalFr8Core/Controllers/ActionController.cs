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
        private const string curPlugin = "terminalFr8Core";
        private BasePluginController _basePluginController = new BasePluginController();

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            ActionDO submittedActionDO = Mapper.Map<ActionDO>(curActionDTO);

            var resultActionDO = await (Task<ActionDO>)_basePluginController.HandleFr8Request(curPlugin, "Configure", submittedActionDO);

            var resultActionDTO = Mapper.Map<ActionDTO>(resultActionDO);

            return resultActionDTO;
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDataPackage)
        {
            ActionDO submittedActionDO = Mapper.Map<ActionDO>(curActionDataPackage);

            var resultActionDO = _basePluginController.HandleFr8Request(curPlugin, "Activate", submittedActionDO);

            var resultActionDTO = Mapper.Map<ActionDTO>(resultActionDO);

            return resultActionDTO;
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDataPackage)
        {
            ActionDO submittedActionDO = Mapper.Map<ActionDO>(curActionDataPackage);

            var resultActionDO = _basePluginController.HandleFr8Request(curPlugin, "Deactivate", submittedActionDO);

            var resultActionDTO = Mapper.Map<ActionDTO>(resultActionDO);

            return resultActionDTO;
        }

        [HttpPost]
        [Route("run")]
        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            ActionDO submittedActionDO = Mapper.Map<ActionDO>(curActionDTO);

            var resultPayloadDTO = await (Task<PayloadDTO>)_basePluginController.HandleFr8Request(curPlugin, "Run", submittedActionDO);

            return resultPayloadDTO;

        }
    }
}