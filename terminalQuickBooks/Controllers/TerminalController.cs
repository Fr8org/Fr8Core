using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using Newtonsoft.Json;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Services;
using TerminalBase.BaseClasses;
using Utilities.Configuration.Azure;
using System.Web.Http.Description;
using Data.Interfaces.Manifests;

namespace terminalQuickBooks.Controllers
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
                Name = "terminalQuickBooks",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                Version = "1"
            };
            var webService = new WebServiceDTO()
            {
                Name = "QuickBooks",
                IconPath = "/Content/icons/web_services/quick-books-icon-64x64.png"
            };
            var createJournalEntryActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Create_Journal_Entry",
                Label = "Create Journal Entry",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                AuthenticationType = AuthenticationType.External,
                MinPaneWidth = 330,
                WebService = webService
            };
            var actionList = new List<ActivityTemplateDTO>()
            {
                createJournalEntryActionTemplate
            };
            var curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Actions = actionList
            };
            return Json(curStandardFr8TerminalCM);
        }
    }
}