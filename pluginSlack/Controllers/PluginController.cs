using System;
using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Entities;
using Data.States;

namespace pluginSlack.Controllers
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
            
            var template = new ActivityTemplateDO
            {
                Name = "Publish_To_Slack",
                Category = ActivityCategory.fr8_Forwarder,
                Version = "1"
            };

            var plugin = new PluginDO
            {
                Endpoint = "localhost:39504",
                PluginStatus = PluginStatus.Active,
                Name = "pluginSlack",
                Version = "1"
            };
            
            template.Plugin = plugin;

            result.Add(template);

            return Json(result);    
        }
    }
}