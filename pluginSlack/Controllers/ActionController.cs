using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
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