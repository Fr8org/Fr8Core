using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using fr8.Infrastructure.Data.DataTransferObjects;
using TerminalBase.Services;
using StructureMap;

namespace terminalGoogle.Controllers
{
    [RoutePrefix("activities")]
    public class ActivityController : ApiController
    {
        private const string curTerminal = "terminalGoogle";
        private readonly ActivityExecutor _activityExecutor;
        public ActivityController()
        {
            _activityExecutor = ObjectFactory.GetInstance<ActivityExecutor>();
        }

        [HttpPost]
        public Task<object> Execute([FromUri] String actionType, [FromBody] Fr8DataDTO curDataDTO)
        {
            var queryParams = Request.GetQueryNameValuePairs();
            return _activityExecutor.HandleFr8Request(curTerminal, actionType, queryParams, curDataDTO);
        }
    }
}