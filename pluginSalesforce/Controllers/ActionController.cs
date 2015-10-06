using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using terminalBase.BaseClasses;

namespace pluginSalesforce.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController:ApiController
    {
        private const string curPlugin = "pluginSalesforce";
        private BaseTerminalController _basePluginController = new BaseTerminalController();

        [HttpPost]
        [Route("create")]
        public ActionDTO Create(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "CreateLead", curActionDTO);
        }
    }
}