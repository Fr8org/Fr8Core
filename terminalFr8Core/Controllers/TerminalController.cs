using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Utilities.Configuration.Azure;
using Data.Interfaces.Manifests;
using Data.Constants;

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
                Endpoint = CloudConfigurationManager.GetSetting("terminalFr8Core.TerminalEndpoint"),
                TerminalStatus = TerminalStatus.Active,
                Name = "terminalFr8Core",
                Label = "Fr8Core",
                Version = "1"
            };

            var webService = new WebServiceDTO
            {
                Name = "Built-In Services"
            };

            result.Add(new ActivityTemplateDTO
            {
                Name = "Build_Message",
                Label = "Build a Message",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1"
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "Process_Personnel_Report",
                Label = "Process Personnel Report",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1"
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "TestIncomingData",
                Label = "Test Incoming Data",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                Version = "1",
                MinPaneWidth = 420,
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
                WebService = webService,
                Tags = Tags.Internal
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "MapFields",
                Label = "Map Fields",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                Tags = $"{Tags.AggressiveReload},{Tags.Internal}",
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
                Name = "SaveToFr8Warehouse",
                Label = "Save To Fr8 Warehouse",
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
                MinPaneWidth = 330,
                Tags = Tags.Internal
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "ConnectToSql",
                Label = "Connect To SQL",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                Tags = Tags.Internal
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "BuildQuery",
                Label = "Build Query",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                Tags = Tags.Internal
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "ExecuteSql",
                Label = "Execute Sql Query",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                Tags = Tags.Internal
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "ManagePlan",
                Label = "Manage Plan",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                Tags = Tags.Internal
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
                Label = "Loop",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                Type = ActivityType.Loop,
                Tags = Tags.AggressiveReload
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

            result.Add(new ActivityTemplateDTO()
            {
                Name = "QueryFr8Warehouse",
                Label = "Query Fr8 Warehouse",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                MinPaneWidth = 550
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "Show_Report_Onscreen",
                Label = "Show Report Onscreen",
                Version = "1",
                Category = ActivityCategory.Processors,
                NeedsAuthentication = false,
                Terminal = terminal,
                WebService = webService,
                MinPaneWidth = 380
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "Monitor_Fr8_Events",
                Label = "Monitor Fr8 Events",
                Version = "1",
                Category = ActivityCategory.Monitors,
                NeedsAuthentication = false,
                Terminal = terminal,
                MinPaneWidth = 380,
                WebService = webService,
                Tags = Tags.Internal
            });

            result.Add(new ActivityTemplateDTO()
            {
                Name = "StoreFile",
                Label = "Store File",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                Type = ActivityType.Standard
            });

            result.Add(new ActivityTemplateDTO()
            {
                Name = "GetFileFromFr8Store",
                Label = "Get File From Fr8 Store",
                Category = ActivityCategory.Receivers,
                Terminal = terminal,
                WebService = webService,
                Version = "1",
                Type = ActivityType.Standard
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "SearchFr8Warehouse",
                Label = "Search Fr8 Warehouse",
                Version = "1",
                Category = ActivityCategory.Solution,
                NeedsAuthentication = false,
                Terminal = terminal,
                WebService = webService,
                MinPaneWidth = 400,
                Tags = Tags.HideChildren
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "TestAndBranch",
                Label = "Test and Branch",
                Version = "1",
                Category = ActivityCategory.Processors,
                NeedsAuthentication = false,
                Terminal = terminal,
                WebService = webService,
                MinPaneWidth = 350,
                Tags = Tags.AggressiveReload
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "ExtractTableField",
                Label = "Extract Table Field",
                Version = "1",
                Category = ActivityCategory.Processors,
                NeedsAuthentication = false,
                Terminal = terminal,
                WebService = webService
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "CollectData",
                Label = "Collect Data",
                Version = "1",
                Category = ActivityCategory.Processors,
                NeedsAuthentication = false,
                Terminal = terminal,
                WebService = webService,
                MinPaneWidth = 400
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "FindObjectsThatMatchIncomingMessage",
                Label = "Find Objects That Match Incoming Message",
                Version = "1",
                Category = ActivityCategory.Processors,
                NeedsAuthentication = false,
                Terminal = terminal,
                WebService = webService,
                MinPaneWidth = 350
            });

            var curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Activities = result
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}