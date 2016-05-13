using System.Collections.Generic;
using System.Web.Http;
using Data.States;
using Utilities.Configuration.Azure;
using System.Web.Http.Description;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace terminalPapertrail.Controllers
{
    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof (StandardFr8TerminalCM))]
        public IHttpActionResult DiscoverTerminals()
        {
            var terminal = new TerminalDTO()
            {
                Name = "terminalPapertrail",
                Label = "Papertrail",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("terminalPapertrail.TerminalEndpoint"),
                Version = "1"
            };

            var webService = new WebServiceDTO
            {
                Name = "Papertrail",
                IconPath = "/Content/icons/web_services/papertrail-icon-64x64.png"
            };

            var writeToLogActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Write_To_Log",
                Label = "Write To Log",
                Category = ActivityCategory.Forwarders,
                Terminal = terminal,
                MinPaneWidth = 330,
                WebService = webService
                
            };

            var curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Activities = new List<ActivityTemplateDTO> {writeToLogActionTemplate}
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}