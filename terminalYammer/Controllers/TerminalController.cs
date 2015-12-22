using System;
using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Utilities.Configuration.Azure;
using Data.Interfaces.Manifests;

namespace terminalYammer.Controllers
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
                Name = "terminalYammer",
                Version = "1"
            };

            var webService = new WebServiceDTO
            {
                Name = "Yammer",
                IconPath = "/Content/icons/web_services/yammer-64x64.png"
            };

            var postToYammerAction = new ActivityTemplateDTO
            {
                Name = "Post_To_Yammer",
                Label = "Post To Yammer",
                Tags = "Notifier",
                Category = ActivityCategory.Forwarders,
                Terminal = terminal,
                AuthenticationType = AuthenticationType.External,
                Version = "1",
                Description = "Post To Yammer: Description",
                MinPaneWidth = 330,
                WebService = webService
            };

            var result = new List<ActivityTemplateDTO>()
            {
                postToYammerAction
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