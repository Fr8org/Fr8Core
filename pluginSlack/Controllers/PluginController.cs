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
            var plugin = new PluginDO
            {
                Endpoint = "localhost:39504",
                PluginStatus = PluginStatus.Active,
                Name = "pluginSlack",
                RequiresAuthentication = true,
                Version = "1"
            };

            var monitorChannelAction = new ActivityTemplateDO
            {
                Name = "Monitor_Channel",
                Label = "Monitor Channel",
                Category = ActivityCategory.Monitors,
                Plugin = plugin,
                Version = "1",
                MinPaneWidth = 330
            };

            var publishToSlackAction = new ActivityTemplateDO
            {
                Name = "Publish_To_Slack",
                Label = "Publish To Slack",
                Category = ActivityCategory.Forwarders,
                Plugin = plugin,
                Version = "1",
                MinPaneWidth = 330
            };

            var result = new List<ActivityTemplateDO>()
            {
                monitorChannelAction,
                publishToSlackAction
            };

            return Json(result);    
        }
    }
}