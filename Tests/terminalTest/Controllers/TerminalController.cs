using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Utilities.Configuration.Azure;
using Data.Interfaces.Manifests;

namespace terminalTest.Controllers
{
    // This terminal contains activities that simplify core logic manual testing
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
                Endpoint = CloudConfigurationManager.GetSetting("terminalTest.TerminalEndpoint"),
                TerminalStatus = TerminalStatus.Active,
                Name = "terminalTest",
                Version = "1"
            };

            var webService = new WebServiceDTO
            {
                Name = "Terminal for debugging"
            };

            result.Add(new ActivityTemplateDTO
            {
                Name = "SimpleActivity",
                Label = "SimpleActivity",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1"
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "SimpleHierarchicalActivity",
                Label = "SimpleHierarchicalActivity",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1"
            });

            result.Add(new ActivityTemplateDTO
            {
                Name = "SimpleJumperActivity",
                Label = "SimpleJumperActivity",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1"
            });


            result.Add(new ActivityTemplateDTO
            {
                Name = "GenerateTableActivity",
                Label = "GenerateTableActivity",
                Category = ActivityCategory.Processors,
                Terminal = terminal,
                WebService = webService,
                Version = "1"
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