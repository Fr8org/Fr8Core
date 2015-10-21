using System;
using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Data.Entities;
using Data.States;
using Utilities.Configuration.Azure;

namespace terminalAzure.Controllers
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
                Label = "Write to Azure Sql Server",
                Category = ActivityCategory.Forwarders,
                Version = "1"
            };

            var plugin = new PluginDO
            {
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                PluginStatus = PluginStatus.Active,
                Name = "terminalAzure",
                RequiresAuthentication = false,
                Version = "1"
            };
            
            template.Plugin = plugin;

            result.Add(template);

            return Json(result);    
        }
    }
}