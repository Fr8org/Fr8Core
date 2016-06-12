using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;

namespace Fr8.TerminalBase.BaseClasses
{
    public abstract class DefaultActivityController : ApiController
    {
        private readonly IActivityExecutor _activityExecutor;

        protected DefaultActivityController(IActivityExecutor activityExecutor)
        {
            _activityExecutor = activityExecutor;
        }

        [HttpPost]
        public Task<object> Execute([FromUri] String actionType, [FromBody] Fr8DataDTO curDataDTO)
        {
            var queryParams = Request.GetQueryNameValuePairs();
            return _activityExecutor.HandleFr8Request(actionType, queryParams, curDataDTO);
        }
    }
}
