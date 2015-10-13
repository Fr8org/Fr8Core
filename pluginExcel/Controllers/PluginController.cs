using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Entities;
using Data.States;

namespace pluginExcel.Controllers
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
                Endpoint = "localhost:47011",
                PluginStatus = PluginStatus.Active,
                Name = "pluginExcel",
                Version = "1"
            };

            var template = new ActivityTemplateDO
            {
                Name = "Extract_Data",
                Version = "1",
                Category = ActivityCategory.fr8_Receiver,
                Plugin = plugin
            };

           

            result.Add(template);

            return Json(result);    
        }
    }
}