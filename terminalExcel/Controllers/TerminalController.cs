using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Entities;
using Data.States;
using Utilities.Configuration.Azure;
using Utilities.Configuration.Azure;
using Data.Interfaces.Manifests;

namespace terminalExcel.Controllers
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
            var result = new List<ActivityTemplateDO>();

            var terminal = new TerminalDO
            {
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                TerminalStatus = TerminalStatus.Active,
                Name = "terminalExcel",
                Version = "1"
            };

            result.Add(new ActivityTemplateDO
            {
                Name = "Load_Excel_File",
                Label = "Load Excel File",
                Version = "1",
                Category = ActivityCategory.Receivers,
                Terminal = terminal,
                Tags = "Table Data Generator",
                MinPaneWidth = 210
            });


            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Actions = result
            };
            return Json(curStandardFr8TerminalCM);
        }
    }
}