using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Entities;
using Data.States;
using Utilities.Configuration.Azure;
using Utilities.Configuration.Azure;

namespace terminalExcel.Controllers
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
                Name = "terminalExcel",
                Version = "1"
            };

            var template = new ActivityTemplateDO
            {
                Name = "Extract_Data",
                Label = "Extract Data",
                Version = "1",
                Category = ActivityCategory.Receivers,
                Plugin = plugin,
                MinPaneWidth = 210
            };

           

            result.Add(template);

            return Json(result);    
        }
    }
}