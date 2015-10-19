using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Data.Entities;
using PluginBase.BaseClasses;
using pluginSlack.Actions;
using pluginSlack.Interfaces;
using pluginSlack.Services;

namespace pluginSlack.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginSlack";
        private readonly BasePluginController _basePluginController;
        private readonly ISlackIntegration _slackIntegration;

        public ActionController()
        {
            _basePluginController = new BasePluginController();
            _slackIntegration = new SlackIntegration();
        }

        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>) _basePluginController
                .HandleDockyardRequest(curPlugin, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public string Activate(ActionDTO curActionDataPackage)
        {
            return string.Empty;
        }

        [HttpPost]
        [Route("deactivate")]
        public string Deactivate(ActionDTO curActionDataPackage)
        {
            return string.Empty;
        }

        [HttpPost]
        [Route("run")]
        public async Task<PayloadDTO> Run(ActionDTO actionDto)
        {
            return await (Task<PayloadDTO>)_basePluginController.HandleDockyardRequest(
                curPlugin, "Run", actionDto);
        }

        [HttpPost]
        [Route("auth_url")]
        public ExternalAuthUrlDTO GetExternalAuthUrl()
        {
            var externalStateToken = Guid.NewGuid().ToString();
            var url = _slackIntegration.CreateAuthUrl(externalStateToken);

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

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                throw new ApplicationException("Code or State is empty.");
            }

            var oauthToken = await _slackIntegration.GetOAuthToken(code);
            var userId = await _slackIntegration.GetUserId(oauthToken);

            return new AuthTokenDTO()
            {
                Token = oauthToken,
                ExternalAccountId = userId,
                ExternalStateToken = state
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

        // TODO: do we need this?
        // [HttpPost]
        // [Route("Publish_To_Slack")]
        // public IHttpActionResult PublishToSlack(SlackPayloadDTO curSlackPayload)
        // {
        //     return Ok(_actionHandler.Execute(curSlackPayload));
        // }
    }
}