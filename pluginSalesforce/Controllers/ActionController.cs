using Data.Interfaces.DataTransferObjects;
using System.Web.Http;
using terminal_base.BaseClasses;

namespace terminal_Salesforce.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController:ApiController
    {
        private const string _curTerminal = "terminal_Salesforce";
        private BaseTerminalController _baseTerminalController = new BaseTerminalController();

        [HttpPost]
        [Route("create")]
        public ActionDTO Create(ActionDTO curActionDTO)
        {
            return (ActionDTO)_baseTerminalController.HandleDockyardRequest(_curTerminal, "CreateLead", curActionDTO);
        }
    }
}