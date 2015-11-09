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
        private const string curTerminal = "terminalSalesforce";
        private BaseTerminalController _baseTerminalController = new BaseTerminalController();
        private ISalesforceIntegration _salesforceIntegration = new SalesforceIntegration();

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>)_baseTerminalController
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
        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            return await (Task<PayloadDTO>)_baseTerminalController.HandleDockyardRequest(curTerminal, "Run", curActionDTO);
        }
    }
}