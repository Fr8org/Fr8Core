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
        public string Configure(ActionDO curActionDO)
        {
            return _basePluginController.HandleDockyardRequest(curPlugin, "Configure", curActionDO);
        }

        [HttpPost]
        [Route("activate")]
        public string Activate(ActionDO curActionDO)
        {
            return _basePluginController.HandleDockyardRequest(curPlugin, "Activate", curActionDO);
        }

        [HttpPost]
        [Route("execute")]
        public string Execute(ActionDO curActionDO)
        {
            return _basePluginController.HandleDockyardRequest(curPlugin, "Execute", curActionDO);
        }
    }
}