using System;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;

namespace terminalAtlassian.Controllers
{
    [RoutePrefix("activities")]
    public class ActivityController: ApiController
    {
        private const string curTerminal = "terminalAtlassian";
        private readonly ActivityExecutor _activityExecutor;

        public ActivityController(ActivityExecutor activityExecutor)
        {
            _activityExecutor = activityExecutor;
        }

        [HttpPost]
        public Task<object> Execute([FromUri] String actionType, [FromBody] Fr8DataDTO curDataDTO)
        {
            var queryParams = Request.GetQueryNameValuePairs();
            return _activityExecutor.HandleFr8Request(curTerminal, actionType, queryParams, curDataDTO);
        }
    }
}