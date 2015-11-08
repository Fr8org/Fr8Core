using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using Data.Entities;
using Data.Interfaces.Manifests;
using Data.States;
using Utilities.Configuration.Azure;

namespace terminalGoogle.Controllers
{
    [RoutePrefix("plugins")]
    public class PluginController : ApiController
    {
        /// <summary>
        /// Plugin discovery infrastructure.
        /// Action returns list of supported actions by plugin.
        /// </summary>
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult DiscoverPlugins()
        {
            var plugin = new PluginDO
            {
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                PluginStatus = PluginStatus.Active,
                Name = "terminalGoogle",
                Version = "1"
            };

            var extractDataAction = new ActivityTemplateDO
            {
                Name = "Extract_Spreadsheet_Data",
                Label = "Extract Spreadsheet Data",
                Version = "1",
                Category = ActivityCategory.Receivers,
                Plugin = plugin,
                AuthenticationType = AuthenticationType.External,
                MinPaneWidth = 300
            };

            return Json(new StandardFr8TerminalCM()
            {
                Definition = plugin,
                Actions = new List<ActivityTemplateDO>
                {
                    extractDataAction
                }
            });    
        }
    }
}