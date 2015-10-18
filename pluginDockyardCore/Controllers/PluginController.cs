using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Core.Services;
using Data.Entities;
using Data.States;

namespace pluginDockyardCore.Controllers
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
                Endpoint = "localhost:50705",
                PluginStatus = PluginStatus.Active,
                Name = "pluginDockyardCore",
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

            return Json(result);    
        }
    }
}