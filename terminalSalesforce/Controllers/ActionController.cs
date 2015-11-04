using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;
using Salesforce.Common;

namespace terminalSalesforce.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController:ApiController
    {
        private const string curPlugin = "terminalSalesforce";
        private BasePluginController _basePluginController = new BasePluginController();
        private ISalesforceIntegration _salesforceIntegration = new SalesforceIntegration();

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