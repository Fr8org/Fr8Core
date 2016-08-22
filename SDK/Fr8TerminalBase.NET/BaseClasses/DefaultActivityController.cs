using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Services;
using Swashbuckle.Swagger.Annotations;

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
        /// <summary>
        /// This endpoint supports the folowing actions: <em>/configure</em>,<em>/run</em>, <em>/activate</em>, <em>/deactivate</em> and <em>/documentation</em>. Click <a target="_blank" href="https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/PlatformIdependentTerminalDeveloperGuide.md" style="text-decoration: underline; color: blue;">here</a> for more information
        /// </summary>
        /// <param name="actionType">Action to execute. Available values are "configure", "run", "activate", "deactivate" and "documentation"</param>
        /// <param name="curDataDTO">Object containing info about activity being processed</param>
        [SwaggerResponse(HttpStatusCode.OK, "Specified action was successfully executed. The response content depends on the action specified. For more details see documentation above")]
        [HttpPost]
        public Task<object> Execute([FromUri] String actionType, [FromBody] Fr8DataDTO curDataDTO)
        {
            var queryParams = Request.GetQueryNameValuePairs();
            return _activityExecutor.HandleFr8Request(actionType, queryParams, curDataDTO);
        }
    }
}
