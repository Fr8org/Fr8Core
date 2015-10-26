using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using Data.Entities;
using Data.States;
using Utilities.Configuration.Azure;

namespace terminalGoogle.Controllers
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
                Name = "terminalGoogle",
                Version = "1"
            };

            result.Add(new ActivityTemplateDO
            {
                Name = "Extract_Spreadsheet_Data",
                Label = "Extract Spreadsheet Data",
                Version = "1",
                Category = ActivityCategory.Receivers,
                Plugin = plugin,
                AuthenticationType = AuthenticationType.External,
                MinPaneWidth = 210
            });
            return Json(result);    
        }
    }
}