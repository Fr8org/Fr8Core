using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Utilities.Configuration.Azure;
using Data.Interfaces.Manifests;

namespace terminalFr8Core.Controllers
{

    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        /// <summary>
        /// Terminal discovery infrastructure.
        /// Action returns list of supported actions by terminal.

        /// </summary>
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]

        public IHttpActionResult DiscoverTerminals()
        {
            var result = new List<ActivityTemplateDTO>();

            var terminal = new TerminalDTO
            {
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                TerminalStatus = TerminalStatus.Active,
                Name = "terminalFr8Core",
                Version = "1"
            };

            var webService = new WebServiceDTO
            {
                Name = "fr8 Core"
            };

            result.Add(new ActivityTemplateDTO
            {
                Name = "FilterUsingRunTimeData",
                Label = "Filter Using Runtime Data",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                Version = "1",
                MinPaneWidth = 330,
                WebService = webService
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "ConvertCrates",
                Label = "Convert Crates",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                Version = "1",
                MinPaneWidth = 330,
                WebService = webService
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "MapFields",
                Label = "Map Fields",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                Tags = "AggressiveReload",
                Version = "1",
                MinPaneWidth = 380,
                WebService = webService
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "AddPayloadManually",
                Label = "Add Payload Manually",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                Version = "1",
                MinPaneWidth = 330,
                WebService = webService
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "StoreMTData",
                Label = "Store MT Data",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1"
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "Select_Fr8_Object",
                Label = "Select Fr8 Object",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                MinPaneWidth = 330
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "ConnectToSql",
                Label = "Connect To SQL",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1"
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "BuildQuery",
                Label = "Build Query",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1"
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "ExecuteSql",
                Label = "Execute Sql Query",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1"
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "ManageRoute",
                Label = "Manage Route",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1"
            });

            result.Add(new ActivityTemplateDTO()
            {
                Name = "FindObjects_Solution",
                Label = "Find Objects Solution",
                Category = ActivityCategory.Solution,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                Type = ActivityType.Solution
            });

            result.Add(new ActivityTemplateDTO()
            {
                Name = "Loop",
                Label = "Fr8 Core Loop",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                Type = ActivityType.Loop
            });

            result.Add(new ActivityTemplateDTO()
            {
                Name = "SetDelay",
                Label = "Delay Action Processing",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                Type = ActivityType.Standard
            });

            result.Add(new ActivityTemplateDTO()
            {
                Name = "ConvertRelatedFieldsIntoTable",
                Label = "Convert Related Fields Into a Table",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                MinPaneWidth = 400
            });

            var curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Actions = result
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}