﻿using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8Data.DataTransferObjects;
using TerminalBase.Infrastructure;
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
            return _activityExecutor.HandleFr8Request(curTerminal, actionType, curDataDTO);
        }
    }
}