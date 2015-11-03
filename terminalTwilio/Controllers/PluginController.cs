using System.Collections.Generic;
using System.Web.Http;
using Data.Entities;
using Data.States;
using System.Web.Http.Description;
using Utilities.Configuration.Azure;
using Data.Interfaces.Manifests;

namespace terminalTwilio.Controllers
{
    [RoutePrefix("plugins")]
    public class PluginController : ApiController
    {
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult DiscoverPlugins()
        {
            var plugin = new PluginDO()
            {
                Name = "terminalTwilio",
                PluginStatus = PluginStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                Version = "1"
            };

	        var webService = new WebServiceDO
	        {
		        Name = "Twilio"
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
                MinPaneWidth = 330,
				WebService = webService
            };

            var actionList = new List<ActivityTemplateDO>
            {
                sendViaTwilioTemplate
            };

            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = plugin,
                Actions = actionList
            };
            return Json(curStandardFr8TerminalCM);
        }
    }
}