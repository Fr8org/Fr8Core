using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Entities;
using Data.States;

namespace pluginAzureSqlServer.Controllers
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
                Name = "Write_To_Sql_Server",
                Version = "1"
            };

            var plugin = new PluginDO
            {
                Endpoint = "localhost:46281",
                PluginStatus = PluginStatus.Active,
                Name = "pluginAzureSqlServer"
            };
            
            template.Plugin = plugin;

            result.Add(template);

            return Json(result);    
        }
    }
}