using System;
using System.Web.Http;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;

namespace terminalAzure.Controllers
{
    public class ActivityController : DefaultActivityController
    {
        public ActivityController(IActivityExecutor activityExecutor)
            : base(activityExecutor)
        {
        }

        //----------------------------------------------------------

        [HttpPost]
        [Route("Write_To_Sql_Server")]
        [Obsolete]
        public IHttpActionResult Process(ActivityDTO curActivityDTO)
        {
            return
                Ok("This end point has been deprecated. Please use the V2 mechanisms to POST to this terminal. For more" +
                   "info see https://maginot.atlassian.net/wiki/display/SH/V2+Plugin+Design");
        }

        [HttpPost]
        [Route("Write_To_Sql_Server/{path}")]
        [Obsolete]
        public IHttpActionResult Process(string path, ActivityDTO curActivityDTO)

        {
            return
                Ok("This end point has been deprecated. Please use the V2 mechanisms to POST to this terminal. For more" +
                   "info see https://maginot.atlassian.net/wiki/display/SH/V2+Plugin+Design");
        }
    }
}