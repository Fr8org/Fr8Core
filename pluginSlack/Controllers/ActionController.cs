using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Data.Entities;
using terminalBase.BaseClasses;
using System;
using pluginSlack.Actions;
using StructureMap;

namespace pluginSlack.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginSlack";
        private BaseTerminalController _basePluginController = new BaseTerminalController();

        [HttpPost]
        [Route("configure")]
        public CrateStorageDTO Configure(ActionDTO curActionDataPackage)
        {
            var response = (CrateStorageDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Configure", curActionDataPackage);
            if (response == null)
                response = new CrateStorageDTO();
            return response;
        }

        [HttpPost]
        [Route("activate")]
        public string Activate(ActionDTO curActionDataPackage)
        {
            return string.Empty;
        }

        [HttpPost]
        [Route("deactivate")]
        public string Deactivate(ActionDTO curActionDataPackage)
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