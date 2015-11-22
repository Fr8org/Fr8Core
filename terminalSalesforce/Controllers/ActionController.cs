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
        public async Task<object> Execute([FromUri] String actionType, [FromBody] ActionDTO curActionDTO)
        {
            if (actionType.Equals("run", StringComparison.InvariantCultureIgnoreCase))
                return await (Task<PayloadDTO>)_baseTerminalController.HandleFr8Request(curTerminal, actionType, curActionDTO);
            return await (Task<ActionDTO>)_baseTerminalController.HandleFr8Request(curTerminal, actionType, curActionDTO);
        }
    }
}