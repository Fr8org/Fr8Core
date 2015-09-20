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
        public CrateStorageDTO Configure(ActionDTO curActionDTO)
        {
            return (CrateStorageDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Activate", curActionDTO);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Deactivate", curActionDTO);
        }


        [HttpPost]
        [Route("execute")]
        public ActionDTO Execute(ActionDataPackageDTO curActionDataPackage)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Execute", curActionDataPackage.ActionDTO, curActionDataPackage);
        }
    }
}