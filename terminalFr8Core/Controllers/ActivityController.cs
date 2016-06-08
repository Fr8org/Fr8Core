using System;
using System.Threading.Tasks;
using System.Web.Http;
using fr8.Infrastructure.Data.DataTransferObjects;
using StructureMap;
using TerminalBase.Services;

namespace terminalFr8Core.Controllers
{
    [RoutePrefix("activities")]
    public class ActivityController : ApiController
    {
        private const string curTerminal = "terminalFr8Core";
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