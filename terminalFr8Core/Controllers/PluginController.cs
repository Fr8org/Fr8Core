using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Entities;
using Data.States;
using Hub.Services;
using Utilities.Configuration.Azure;
using Data.Interfaces.Manifests;

namespace terminalFr8Core.Controllers
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
            var result = new List<ActivityTemplateDO>();
            
            var plugin = new PluginDO
            {
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                PluginStatus = PluginStatus.Active,
                Name = "terminalFr8Core",
                Version = "1"
            };

            result.Add(new ActivityTemplateDO
            {
                Name = "FilterUsingRunTimeData",
                Label = "Filter Using Runtime Data",
                Category = ActivityCategory.Processors,
                Plugin = plugin,
                AuthenticationType = AuthenticationType.None,
                Version = "1",
				MinPaneWidth = 330
            });

            result.Add(new ActivityTemplateDO
            {
                Name = "MapFields",
                Label = "Map Fields",
                Category = ActivityCategory.Processors,
                Plugin = plugin,
                AuthenticationType = AuthenticationType.None,
                Version = "1",
				MinPaneWidth = 380
            });

            result.Add(new ActivityTemplateDO
            {
                Name = "AddPayloadManually",
                Label = "Add Payload Manually",
                Category = ActivityCategory.Processors,
                Plugin = plugin,
                AuthenticationType = AuthenticationType.None,
                Version = "1",
				MinPaneWidth = 330
            });

            result.Add(new ActivityTemplateDO
            {
                Name = "StoreMTData",
                Label = "Store MT Data",
                Category = ActivityCategory.Processors,
                Plugin = plugin,
                Version = "1"
            });

            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = plugin,
                Actions = result
            };
            return Json(curStandardFr8TerminalCM);
            result.Add(new ActivityTemplateDO
            {
                Name = "Select_Fr8_Object",
                Label = "Select Fr8 Object",
                Category = ActivityCategory.Processors,
                Plugin = plugin,
                Version = "1",
                MinPaneWidth = 330
            });

            return Json(result);    
        }
    }
}