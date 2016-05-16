using System.Web.Http;
using System.Collections.Generic;
using Data.States;
using Utilities.Configuration.Azure;
using System.Web.Http.Description;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace terminalBox.Controllers
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
                Name = "terminalBox",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("terminalBox.TerminalEndpoint"),
                Version = "1",
                AuthenticationType = AuthenticationType.External
            };

            var webService = new WebServiceDTO
            {
                Name = "Box",
                IconPath = "/Content/icons/web_services/Box-logo_64x64.png"
            };

            var dummyActivity = new ActivityTemplateDTO
            {
                Name = "SaveToFile",
                Label = "SaveToFile",
                Version = "1",
                Category = ActivityCategory.Forwarders,
                Terminal = terminal,
                NeedsAuthentication = true,
                MinPaneWidth = 300,
                WebService = webService
            };

            var test = new ActivityTemplateDTO
            {
                Name = "GenerateTableActivity",
                Label = "GenerateTableActivity",
                Category = ActivityCategory.Receivers,
                Terminal = terminal,
                WebService = webService,
                Version = "1"
            };

            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Activities = new List<ActivityTemplateDTO>
                {
                   dummyActivity,
                   test
                }
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}