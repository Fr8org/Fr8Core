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
    public class ActionController:ApiController
    {
        private const string curPlugin = "pluginSalesforce";
        private BasePluginController _basePluginController = new BasePluginController();

        [HttpPost]
        [Route("create")]
        public ActionDTO Create(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "CreateLead", curActionDTO);
        }
    }
}