using System.Web.Http;
using Data.Entities;
using System.Collections.Generic;
using Data.States;
using Utilities.Configuration.Azure;
using System.Web.Http.Description;
<<<<<<< HEAD
using Data.Interfaces.DataTransferObjects;
=======
>>>>>>> dev
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
<<<<<<< HEAD
            var terminal = new TerminalDTO()
=======
            var terminal = new TerminalDO()
>>>>>>> dev
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
<<<<<<< HEAD
            var getFileListAction = new ActivityTemplateDTO()
=======
            var getFileListAction = new ActivityTemplateDO()
>>>>>>> dev
            {
                Version = "1",
                Name = "Get_File_List",
                Label = "Get File List",
                Terminal = terminal,
                AuthenticationType = AuthenticationType.External,
<<<<<<< HEAD
                Category = ActivityCategory.Forwarders.ToString(),
                MinPaneWidth = 330,
                WebServiceName = webService.Name
            };
            var actionList = new List<ActivityTemplateDTO>()
=======
                Category = ActivityCategory.Forwarders,
                MinPaneWidth = 330,
                WebService = webService
            };
            var actionList = new List<ActivityTemplateDO>()
>>>>>>> dev
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