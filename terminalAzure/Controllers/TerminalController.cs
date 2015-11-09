using System;
using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Entities;
using Data.States;
using Utilities.Configuration.Azure;
using Data.Interfaces.Manifests;

namespace terminalAzure.Controllers
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
            var result = new List<ActivityTemplateDO>();

            var template = new ActivityTemplateDO
            {
                Name = "Write_To_Sql_Server",
                Label = "Write to Azure Sql Server",
                Category = ActivityCategory.Forwarders,
                AuthenticationType = AuthenticationType.None,
                Version = "1",
                MinPaneWidth = 330
            };

            var plugin = new TerminalDO
            {
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                TerminalStatus = TerminalStatus.Active,
                Name = "terminalAzure",
                Version = "1"
            };

            template.Terminal = plugin;

            result.Add(template);

            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
             {
                 Definition = plugin,
                 Actions = result
             };
            return Json(curStandardFr8TerminalCM);
        }
    }
}