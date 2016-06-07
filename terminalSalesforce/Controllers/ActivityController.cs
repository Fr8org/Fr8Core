using System;
using System.Web.Http;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using StructureMap;
using TerminalBase.Infrastructure;
using TerminalBase.Services;

namespace terminalSalesforce.Controllers
{
    [RoutePrefix("activities")]
    public class ActivityController: ApiController
    {
        private const string curTerminal = "terminalSalesforce";
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