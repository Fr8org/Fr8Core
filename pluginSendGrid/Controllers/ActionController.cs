using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using PluginBase.BaseClasses;

namespace pluginSalesforce.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginSendGrid";
        private BasePluginController _basePluginController = new BasePluginController();

        [HttpPost]
        [Route("configure")]
        public ActionDTO Configure(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public string Activate(ActionDTO curActionDataPackage)
        {
            return string.Empty;
        }

        [HttpPost]
        [Route("deactivate")]
        public string Deactivate(ActionDTO curActionDataPackage)
        {
            return string.Empty;
        }

        [HttpPost]
        [Route("execute")]
        public string Execute(ActionDTO curActionDataPackage)
        {
            return string.Empty;
        }
    }
}