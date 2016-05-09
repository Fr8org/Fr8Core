using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using Data.States;
using Fr8Data.Constants;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Utilities.Configuration.Azure;

namespace terminalSendGrid.Controllers
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
                Name = "terminalSendGrid",
                Label = "SendGrid",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("terminalSendGrid.TerminalEndpoint"),
                Version = "1"
            };

	        var webService = new WebServiceDTO
	        {
		        Name = "SendGrid"
	        };

            var activity = new ActivityTemplateDTO()
            {
                Name = "SendEmailViaSendGrid",
                Label = "Send Email",
                Version = "1",
                Tags = string.Join(",", Tags.Notifier, Tags.EmailDeliverer),
                Terminal = terminal,
                Category = ActivityCategory.Forwarders,
                MinPaneWidth = 330,
                WebService = webService
            };

            var actionList = new List<ActivityTemplateDTO>()
            {
                activity
            };

            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Activities = actionList
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}