using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using PluginBase.BaseClasses;
using System.Threading.Tasks;
using pluginSalesforce.Infrastructure;
using pluginSalesforce.Services;

namespace pluginSalesforce.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController:ApiController
    {
        private const string curPlugin = "pluginSalesforce";
        private BasePluginController _basePluginController = new BasePluginController();
        private ISalesforceIntegration _salesforceIntegration = new SalesforceIntegration();

        [HttpPost]
        [Route("create")]
        public ActionDTO Create(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "CreateLead", curActionDTO);
        }

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
            var externalStateToken = Guid.NewGuid().ToString();
            var url = _salesforceIntegration.CreateAuthUrl();

            var externalAuthUrlDTO = new ExternalAuthUrlDTO()
            {
                ExternalStateToken = externalStateToken,
                Url = url
            };

            return externalAuthUrlDTO;
        }

        //[HttpPost]
        //[Route("authenticate_external")]
        //public async Task<AuthTokenDTO> Authenticate(
        //    ExternalAuthenticationDTO externalAuthDTO)
        //{
        //    string code;
        //    string state;

        //    ParseCodeAndState(externalAuthDTO.RequestQueryString, out code, out state);

        //    if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        //    {
        //        throw new ApplicationException("Code or State is empty.");
        //    }

        //    var oauthToken = await _slackIntegration.GetOAuthToken(code);
        //    var userId = await _slackIntegration.GetUserId(oauthToken);

        //    return new AuthTokenDTO()
        //    {
        //        Token = oauthToken,
        //        ExternalAccountId = userId,
        //        ExternalStateToken = state
        //    };
        //}
    }
}