using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Core.Services;
using Data.Entities;
using Data.States;
using Utilities.Configuration.Azure;

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
        [ResponseType(typeof(List<ActivityTemplateDO>))]
        public IHttpActionResult DiscoverPlugins()
        {
            var result = new List<ActivityTemplateDO>();
            
            var plugin = new PluginDO
            {
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                PluginStatus = PluginStatus.Active,
                Name = "terminalFr8Core",
                RequiresAuthentication = false,
                Version = "1"
            };

            result.Add(new ActivityTemplateDO
            {
                Name = "FilterUsingRunTimeData",
                Label = "Filter Using Runtime Data",
                Category = ActivityCategory.Processors,
                Plugin = plugin,
                Version = "1"
            });

            result.Add(new ActivityTemplateDO
            {
                Name = "MapFields",
                Label = "Map Fields",
                Category = ActivityCategory.Processors,
                Plugin = plugin,
                Version = "1"
            });

            result.Add(new ActivityTemplateDO
            {
                Name = "AddPayloadManually",
                Label = "Add Payload Manually",
                Category = ActivityCategory.Processors,
                Plugin = plugin,
                Version = "1"
            });

            result.Add(new ActivityTemplateDO
            {
                Name = "StoreMTData",
                Label = "Store MT Data",
                Category = ActivityCategory.Processors,
                Plugin = plugin,
                Version = "1"
            });

            return Json(result);    
        }
    }
}