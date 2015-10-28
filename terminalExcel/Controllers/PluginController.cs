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

            result.Add(new ActivityTemplateDO
            {
                Name = "Load_Table_Data",
                Label = "Load Table Data",
                Version = "1",
                Category = ActivityCategory.Receivers,
                Plugin = plugin,
                Tags = "Table Data Generator",
                MinPaneWidth = 210
            }); 


            return Json(result);    
        }
    }
}