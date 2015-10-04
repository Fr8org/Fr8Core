using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using PluginBase.BaseClasses;

namespace pluginSendGrid.Controllers
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
        [Route("execute")]
        public ActionDTO Execute(ActionDataPackageDTO curActionDataPackage)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Execute", curActionDataPackage.ActionDTO, curActionDataPackage);
        }
    }
}