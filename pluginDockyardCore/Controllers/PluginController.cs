using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
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
                Version = "1"
            };

            result.Add(new ActivityTemplateDO
            {
                Name = "FilterUsingRunTimeData",
                Plugin = plugin,
                Version = "1"
            });

            result.Add(new ActivityTemplateDO
            {
                Name = "MapFields",
                Plugin = plugin,
                Version = "1"
            });

            return Json(result);    
        }
    }
}