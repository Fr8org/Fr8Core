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

namespace pluginSlack.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginSlack";
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
        [Route("execute")]
        public string Execute(ActionDTO curActionDataPackage)
        {
            return string.Empty;
        }

        [HttpPost]
        [Route("auth_url")]
        public ExternalAuthUrlDTO GetExternalAuthUrl()
        {
            var externalStateToken = Guid.NewGuid().ToString();
            var url = CreateAuthUrl(externalStateToken);

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

            var oauthToken = await FetchOAuthToken(code);
            var userId = await FetchUserId(oauthToken);

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

        private async Task<string> FetchOAuthToken(string code)
        {
            var template = ConfigurationManager.AppSettings["SlackOAuthAccessUrl"];
            var url = template.Replace("%CODE%", code);

            var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(url))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonObj = JsonConvert.DeserializeObject<JObject>(responseString);

                return jsonObj.Value<string>("access_token");
            }
        }

        private async Task<string> FetchUserId(string oauthToken)
        {
            var template = ConfigurationManager.AppSettings["SlackAuthTestUrl"];
            var url = template.Replace("%TOKEN%", oauthToken);

            var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(url))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonObj = JsonConvert.DeserializeObject<JObject>(responseString);

                return jsonObj.Value<string>("user_id");
            }
        }

        /// <summary>
        /// Build external Slack OAuth url.
        /// </summary>
        private string CreateAuthUrl(string externalStateToken)
        {
            var template = ConfigurationManager.AppSettings["SlackOAuthUrl"];
            var url = template.Replace("%STATE%", externalStateToken);

            return url;
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