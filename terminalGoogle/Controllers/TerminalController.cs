using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using Data.Entities;
using Data.Interfaces.Manifests;
using Data.States;
using Google.GData.Extensions;
using Utilities.Configuration.Azure;

namespace terminalGoogle.Controllers
{
    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        /// <summary>
        /// Plugin discovery infrastructure.
        /// Action returns list of supported actions by plugin.
        /// </summary>
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult DiscoverTerminals()
        {
            var webService = new WebServiceDO
            {
                Name = "Google",
                IconPath= "/Content/icons/web_services/google-icon-64x64.png"
            };

            var terminal = new TerminalDO()
            {
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                TerminalStatus = TerminalStatus.Active,
                Name = "terminalGoogle",
                Version = "1",
            };

            var extractDataAction = new ActivityTemplateDO
            {
                Name = "Extract_Spreadsheet_Data",
                Label = "Extract Spreadsheet Data",
                Version = "1",
                Description = "Extract Spreadsheet Data: Description",
                Category = ActivityCategory.Receivers,
                Terminal = terminal,
                AuthenticationType = AuthenticationType.External,
                MinPaneWidth = 300,
                WebService = webService,
                Tags = "Table Data Generator"
            };

            return Json(new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Actions = new List<ActivityTemplateDO>
                {
                    extractDataAction
                }
            });    
        }
    }
}