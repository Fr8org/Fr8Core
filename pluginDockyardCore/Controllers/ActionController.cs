using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Newtonsoft.Json;
using StructureMap;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;

namespace pluginDockyardCore.Controllers
{
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginDockyardCore";
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
        public string Execute(ActionDTO curActionDTO)
        {
            return _basePluginController.HandleDockyardRequest(curPlugin, "Execute", curActionDTO);
        }
    }
}