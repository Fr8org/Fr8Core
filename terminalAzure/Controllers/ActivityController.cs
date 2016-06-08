using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using TerminalBase.BaseClasses;
using AutoMapper;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using StructureMap;
using TerminalBase.Infrastructure;
using TerminalBase.Services;

namespace terminalAzure.Controllers
{    
    [RoutePrefix("activities")]
    public class ActivityController : ApiController
    {
        private const string curTerminal = "terminalAzure";
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