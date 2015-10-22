using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using PluginBase.BaseClasses;
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
        private Authentication _authentication = new Authentication();

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>)_basePluginController
                .HandleDockyardRequest(curPlugin, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("auth_url")]
        public ExternalAuthUrlDTO GetExternalAuthUrl()
        {
           return _authentication.GetExternalAuthUrl();
        }

        [HttpPost]
        [Route("authenticate_external")]
        public async Task<AuthTokenDTO> Authenticate(
            ExternalAuthenticationDTO externalAuthDTO)
        {
            return _authentication.Authenticate(externalAuthDTO);
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
        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            return await (Task<PayloadDTO>)_basePluginController.HandleDockyardRequest(curPlugin, "Run", curActionDTO);
        }
    }
}