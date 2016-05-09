using System.Web.Http;
using System.Collections.Generic;
using Data.States;
using Utilities.Configuration.Azure;
using System.Web.Http.Description;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace terminalDropbox.Controllers
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
                Name = "terminalDropbox",
                Label = "Dropbox",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("terminalDropbox.TerminalEndpoint"),
                Version = "1",
                AuthenticationType = AuthenticationType.External
            };

            var webService = new WebServiceDTO
            {
                Name = "Dropbox",
                IconPath = "/Content/icons/web_services/dropbox-icon-64x64.png"
            };

            var getFileListAction = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Get_File_List",
                Label = "Get File List",
                Terminal = terminal,
                NeedsAuthentication = true,
                Category = ActivityCategory.Receivers,
                MinPaneWidth = 330,
                WebService = webService
            };

            var actionList = new List<ActivityTemplateDTO>()
            {
                getFileListAction
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