using Data.Interfaces.DataTransferObjects;
using StructureMap;
using System.Web.Http;
using terminal_base.BaseClasses;
using terminal_Slack.Actions;

namespace terminal_Slack.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string _curTerminal = "terrminal_Slack";
        private BaseTerminalController _baseTerminalController = new BaseTerminalController();

        [HttpPost]
        [Route("configure")]
        public CrateStorageDTO Configure(ActionDTO curActionDataPackage)
        {
            var response = (CrateStorageDTO)_baseTerminalController.HandleDockyardRequest(_curTerminal, "Configure", curActionDataPackage);
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