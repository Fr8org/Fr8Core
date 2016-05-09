using System.Web.Http;
using Data.Entities;
using System.Collections.Generic;
using Data.States;
using Utilities.Configuration.Azure;
using System.Web.Http.Description;
using Data.Interfaces.Manifests;
using Data.Interfaces.DataTransferObjects;

namespace terminalAtlassian.Controllers
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
                Name = "terminalAtlassian",
                Label = "Atlassian",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("terminalAtlassian.TerminalEndpoint"),
                Version = "1",
                AuthenticationType = AuthenticationType.InternalWithDomain
            };

            var webService = new WebServiceDTO
            {
                Name = "Atlassian",
                IconPath = "/Content/icons/web_services/jira-icon-64x64.png"
            };

            var getJiraIssueAction = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Get_Jira_Issue",
                Label = "Get Jira Issue",
                Terminal = terminal,
                NeedsAuthentication = true,
                Category = ActivityCategory.Receivers,
                MinPaneWidth = 330,
                WebService = webService
            };

            var saveJiraIssueActivity = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Save_Jira_Issue",
                Label = "Save Jira Issue",
                Terminal = terminal,
                NeedsAuthentication = true,
                Category = ActivityCategory.Forwarders,
                MinPaneWidth = 330,
                WebService = webService
            };

            var actionList = new List<ActivityTemplateDTO>()
            {
                getJiraIssueAction,
                saveJiraIssueActivity
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