using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http;
using Core.Services;
using Data.Entities;
using Data.States;

namespace terminal_fr8Core.Controllers
{
    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        /// <summary>
        /// Terminal discovery infrastructure.
        /// Action returns list of supported actions by terminal.
        /// </summary>
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(List<ActivityTemplateDO>))]
        public IHttpActionResult DiscoverTerminals()
        {
            var result = new List<ActivityTemplateDO>();
            
            var terminal = new PluginDO
            {
                Endpoint = "localhost:50705",
                PluginStatus = PluginStatus.Active,
                Name = "terminal_fr8Core",
                RequiresAuthentication = false,
                Version = "1"
            };

            result.Add(new ActivityTemplateDO
            {
                Name = "FilterUsingRunTimeData",
                Category = ActivityCategory.fr8_Processor,
                Plugin = terminal,
                Version = "1"
            });

            result.Add(new ActivityTemplateDO
            {
                Name = "MapFields",
                Category = ActivityCategory.fr8_Processor,
                Plugin = terminal,
                Version = "1"
            });

            return Json(result);    
        }
    }
}