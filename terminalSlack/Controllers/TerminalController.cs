using System;
using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Utilities.Configuration.Azure;
using Data.Interfaces.Manifests;

namespace terminalSlack.Controllers
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
            var terminal = new TerminalDTO
            {
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                TerminalStatus = TerminalStatus.Active,
                Name = "terminalSlack",
                Version = "1",
                AuthenticationType = AuthenticationType.External
            };

            var monitorChannelAction = new ActivityTemplateDTO
            {
                Name = "Monitor_Channel",
                Label = "Monitor Channel",
                Category = ActivityCategory.Monitors,
                Terminal = terminal,
                NeedsAuthentication = true,
                Version = "1",
                MinPaneWidth = 330
            };

            var publishToSlackAction = new ActivityTemplateDTO
            {
                Name = "Publish_To_Slack",
                Label = "Publish To Slack",
                Tags = "Notifier",
                Category = ActivityCategory.Forwarders,
                Terminal = terminal,
                NeedsAuthentication = true,
                Version = "1",
                Description = "Publish To Slack: Description",
                MinPaneWidth = 330
            };

            var result = new List<ActivityTemplateDTO>()
            {
                monitorChannelAction,
                publishToSlackAction
            };

            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Actions = result
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}