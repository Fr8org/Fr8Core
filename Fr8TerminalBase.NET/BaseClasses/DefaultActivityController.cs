using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;

namespace Fr8.TerminalBase.BaseClasses
{
    /// <summary>
    /// Base class for Web API controller that are intended to process activity related requests from the Hub. 
    /// See https://github.com/Fr8org/Fr8Core/blob/dev/Docs/ForDevelopers/SDK/.NET/Reference/DefaultActivityController.md
    /// </summary>
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
