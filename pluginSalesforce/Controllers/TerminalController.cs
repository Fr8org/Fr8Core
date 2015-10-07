using Data.Entities;
using Data.States;
using System.Collections.Generic;
using System.Web.Http;
namespace terminal_Salesforce.Controllers
{
    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        [HttpGet]
        [Route("discover")]
        public IHttpActionResult Get()
        {
            var terminal = new PluginDO()
            {
                Name = "terminal_Salesforce",
                PluginStatus = PluginStatus.Active,
                Endpoint = "localhost:51234",
                Version = "1"
            };

            var action = new ActivityTemplateDO()
            {
                Version = "1",
                Name = "Create_Lead",
                Plugin = terminal,
                Category = ActivityCategory.fr8_Forwarder
            };

            var actionList = new List<ActivityTemplateDO>()
            {
                action
            };

            return Ok(actionList);
        }
    }
}