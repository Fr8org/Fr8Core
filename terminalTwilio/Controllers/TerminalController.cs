using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using Data.States;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Utilities.Configuration.Azure;

namespace terminalTwilio.Controllers
{
    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult DiscoverTerminals()
        {
            var terminal = new TerminalDTO
            {
                Name = "terminalTwilio",
                Label = "Twilio",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("terminalTwilio.TerminalEndpoint"),
                Version = "1",
                AuthenticationType = AuthenticationType.None
            };

	        var webService = new WebServiceDTO
	        {
		        Name = "Twilio",
                IconPath = "/Content/icons/web_services/twilio-icon-64x64.png"
            };

            var sendViaTwilioTemplate = new ActivityTemplateDTO
            {
                Name = "Send_Via_Twilio",
                Label = "Send SMS",
                Tags = "Twillio,Notifier",
                Category = ActivityCategory.Forwarders,
                Version = "1",
                Terminal = terminal,
                MinPaneWidth = 330,
                WebService = webService
            };

            var actionList = new List<ActivityTemplateDTO>
            {
                sendViaTwilioTemplate
            };

            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM
            {
                Definition = terminal,
                Activities = actionList
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}