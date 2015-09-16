using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Data.Entities;
using PluginBase.BaseClasses;
using System;
using pluginSlack.Actions;
using StructureMap;

namespace pluginSlack.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginSlack";
        private BasePluginController _basePluginController = new BasePluginController();

        [HttpPost]
        [Route("configure")]
        public string Configure(ActionDTO curActionDataPackage)
        {
            return _basePluginController.HandleDockyardRequest(curPlugin, "Configure", curActionDataPackage);
        }

        [HttpPost]
        [Route("activate")]
        public string Activate(ActionDTO curActionDataPackage)
        {
            return string.Empty;
        }

        [HttpPost]
        [Route("execute")]
        public string Execute(ActionDTO curActionDataPackage)
        {
            return string.Empty;
        }

        [HttpPost]
        [Route("Publish_To_Slack")]
        public IHttpActionResult PublishToSlack(SlackPayloadDTO curSlackPayload)
        {
            var _actionHandler = ObjectFactory.GetInstance<Publish_To_Slack_v1>();
            return Ok(_actionHandler.Execute(curSlackPayload));

        }


    }
}