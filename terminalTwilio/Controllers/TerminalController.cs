using System.Collections.Generic;
using System.Web.Http;
using Data.Entities;
using Data.States;
using System.Web.Http.Description;
using Data.Interfaces.DataTransferObjects;
using Utilities.Configuration.Azure;
using Data.Interfaces.Manifests;

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
            var terminal = new TerminalDTO()
            {
                Name = "terminalTwilio",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                Version = "1"
            };

	        var webService = new WebServiceDTO
	        {
		        Name = "Twilio",
                IconPath = "/Content/icons/web_services/twilio-icon-64x64.png"
            };

            var sendViaTwilioTemplate = new ActivityTemplateDTO
            {
                Name = "Send_Via_Twilio",
                Label = "Send Via Twilio",
                Tags = "Twillio",
                Category = ActivityCategory.Forwarders.ToString(),
                Version = "1",
                Terminal = terminal,
                AuthenticationType = AuthenticationType.None,
                MinPaneWidth = 330,
                WebService = webService
            };

            var actionList = new List<ActivityTemplateDTO>
            {
                sendViaTwilioTemplate
            };

            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Actions = actionList
            };
            return Json(curStandardFr8TerminalCM);
        }
    }
}