using System;
using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Utilities.Configuration.Azure;

namespace terminalAzure.Controllers
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

            var template = new ActivityTemplateDTO
            {
                Name = "Write_To_Sql_Server",
                Label = "Write to Azure Sql Server",
                Category = ActivityCategory.Forwarders,
                Version = "1",
                MinPaneWidth = 330
            };

            var terminal = new TerminalDTO
            {
                Endpoint = CloudConfigurationManager.GetSetting("terminalAzure.TerminalEndpoint"),
                TerminalStatus = TerminalStatus.Active,
                Name = "terminalAzure",
                Label = "Azure",
                Version = "1"
            };

            var webService = new WebServiceDTO
            {
                Name = "Microsoft Azure"
            };


	        template.WebService = webService;
            template.Terminal = terminal;

            result.Add(template);

            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
             {
                 Definition = terminal,
                 Activities = result
             };

            return Json(curStandardFr8TerminalCM);
        }
    }
}