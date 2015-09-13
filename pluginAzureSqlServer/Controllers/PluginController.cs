using Data.Entities;
using Data.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace pluginAzureSqlServer.Controllers
{
    [RoutePrefix("plugins")]
    public class PluginController : ApiController
    {
        [HttpGet]
        [Route("action_templates")]
        public IHttpActionResult ActionTemplates()
        //public JsonResultList<ActionTemplateDO> ActionTemplates()
        //public JsonResult ActionTemplates()
        {
            var result = new List<ActionTemplateDO>();
            var template = new ActionTemplateDO
            {
                Name = "WriteToAzureSqlServer",
                Version = "1.0",
                ActionProcessor = "DockyardAzureSqlServerService",
            };
            var plugin = new PluginDO
            {
                BaseEndPoint = "localhost:46281",
                Endpoint = "localhost:46281",
                PluginStatus = PluginStatus.Active,
                Name = template.Name
            };
            template.Plugin = plugin;
            result.Add(template);
            return Json(result);
        }
    }
}
