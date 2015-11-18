using System.Web.Http;
using Data.Entities;
using System.Collections.Generic;
using Data.States;
using Utilities.Configuration.Azure;
using System.Web.Http.Description;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;

namespace terminalDropbox.Controllers
{
    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult Get()
        {

            var terminal = new TerminalDTO()

            {
                Name = "terminalDropbox",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                Version = "1"
            };
            var webService = new WebServiceDO
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
                AuthenticationType = AuthenticationType.External,

                Category = ActivityCategory.Forwarders.ToString(),
                MinPaneWidth = 330,
                WebServiceName = webService.Name
            };
            var actionList = new List<ActivityTemplateDTO>()

            {
                getFileListAction
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