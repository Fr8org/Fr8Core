using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using PluginBase.BaseClasses;


namespace pluginDocuSign.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginDocuSign";
        private BasePluginController _basePluginController = new BasePluginController();

        [HttpPost]
        [Route("configure")]
        public string Configure(ActionDTO curActionDTO)
        {
            return _basePluginController.HandleDockyardRequest(curPlugin, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public string Activate(ActionDTO curActionDTO)
        {
            return _basePluginController.HandleDockyardRequest(curPlugin, "Activate", curActionDTO);
        }

        [HttpPost]
        [Route("execute")]
        public string Execute(ActionDataPackageDTO curActionDataPackage)
        {
            return _basePluginController.HandleDockyardRequest(curPlugin, "Execute", curActionDataPackage.ActionDTO, curActionDataPackage);
        }
    }
}