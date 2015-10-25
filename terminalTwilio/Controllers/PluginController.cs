using System.Collections.Generic;
using System.Web.Http;
using Data.Entities;
using Data.States;
using System.Web.Http.Description;
using Utilities.Configuration.Azure;

namespace terminalTwilio.Controllers
{
    [RoutePrefix("plugins")]
    public class PluginController : ApiController
    {
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(List<ActivityTemplateDO>))]
        public IHttpActionResult DiscoverPlugins()
        {
            var plugin = new PluginDO()
            {
                Name = "terminalTwilio",
                PluginStatus = PluginStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                Version = "1"
            };

            var sendViaTwilioTemplate = new ActivityTemplateDO
            {
                Name = "Send_Via_Twilio",
                Label = "Send Via Twilio",
                Tags = "Twillio",
                Category = ActivityCategory.Forwarders,
                Version = "1",
                Plugin = plugin,
                AuthenticationType = AuthenticationType.None,
                MinPaneWidth = 330
            };

            var actionList = new List<ActivityTemplateDO>
            {
                sendViaTwilioTemplate
            };

            return Json(actionList);
        }
    }
}