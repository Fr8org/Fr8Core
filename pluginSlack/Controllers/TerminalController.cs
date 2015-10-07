using Data.Entities;
using Data.States;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;

namespace terminal_Slack.Controllers
{
    [RoutePrefix("terminals")]
    public class PluginController : ApiController
    {
        /// <summary>
        /// Plugin discovery infrastructure.
        /// Action returns list of supported actions by plugin.
        /// </summary>
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(List<ActivityTemplateDO>))]
        public IHttpActionResult DiscoverTerminals()
        {
            var result = new List<ActivityTemplateDO>();
            
            var template = new ActivityTemplateDO
            {
                Name = "Publish_To_Slack",
                Category = ActivityCategory.fr8_Forwarder,
                Version = "1"
            };

            var terminal = new PluginDO
            {
                Endpoint = "localhost:39504",
                PluginStatus = PluginStatus.Active,
                Name = "terminal_Slack"
            };

            template.Plugin = terminal;

            result.Add(template);

            return Json(result);    
        }
    }
}