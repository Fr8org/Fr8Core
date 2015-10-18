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
using Salesforce.Common;

namespace pluginSalesforce.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController:ApiController
    {
        private const string curPlugin = "pluginSalesforce";
        private BasePluginController _basePluginController = new BasePluginController();
        private ISalesforceIntegration _salesforceIntegration = new SalesforceIntegration();     

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
            var url = _salesforceIntegration.CreateAuthUrl(externalStateToken);
            var externalAuthUrlDTO = new ExternalAuthUrlDTO()
            {
                ExternalStateToken = externalStateToken,
                Url = url
            };
            return externalAuthUrlDTO;
        }

        [HttpPost]
        [Route("authenticate_external")]
        public async Task<AuthTokenDTO> Authenticate(
            ExternalAuthenticationDTO externalAuthDTO)
        {
            string code;
            string state;

            ParseCodeAndState(externalAuthDTO.RequestQueryString, out code, out state);         

            AuthenticationClient oauthToken = (AuthenticationClient)Task.Run(() => _salesforceIntegration.GetAuthToken(code)).Result;         
          
            return new AuthTokenDTO()
            {
                Token = oauthToken.AccessToken,
                ExternalAccountId = oauthToken.Id.Substring(oauthToken.Id.LastIndexOf("/")+1,oauthToken.Id.Length-(oauthToken.Id.LastIndexOf("/")+1)),
                ExternalStateToken =state,
                ExternalInstanceURL = oauthToken.InstanceUrl,
                ExternalApiVersion = oauthToken.ApiVersion,
                RefreshToken = oauthToken.RefreshToken
            };
        }        

        private void ParseCodeAndState(string queryString, out string code, out string state)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                throw new ApplicationException("QueryString is empty.");
            }

            code = null;
            state = null;

            var tokens = queryString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var nameValueTokens = token.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameValueTokens.Length < 2)
                {
                    continue;
                }

                if (nameValueTokens[0] == "code")
                {
                    code = nameValueTokens[1];
                }
                else if (nameValueTokens[0] == "state")
                {
                    state = nameValueTokens[1];
                }
            }
        }

    }
}