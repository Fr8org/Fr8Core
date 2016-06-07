using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8Data.DataTransferObjects;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using StructureMap;
using TerminalBase.Services;

namespace terminalSlack.Controllers
{
    [RoutePrefix("activities")]
    public class ActivityController : ApiController
    {
        private const string curTerminal = "terminalSlack";
        private readonly ActivityExecutor _activityExecutor;
        public ActivityController()
        {
            _activityExecutor = ObjectFactory.GetInstance<ActivityExecutor>();
        }

        [HttpPost]
        public Task<object> Execute([FromUri] String actionType, [FromBody] Fr8DataDTO curDataDTO)
        {
            return _activityExecutor.HandleFr8Request(curTerminal, actionType, curDataDTO);
        }
    }
}