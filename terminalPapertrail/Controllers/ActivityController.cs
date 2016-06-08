using System;
using System.Threading.Tasks;
using System.Web.Http;
using fr8.Infrastructure.Data.DataTransferObjects;
using TerminalBase.Services;
using StructureMap;

namespace terminalPapertrail.Controllers
{    
    [RoutePrefix("activities")]
    public class ActivityController : ApiController
    {
        private const string curTerminal = "terminalPapertrail";
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