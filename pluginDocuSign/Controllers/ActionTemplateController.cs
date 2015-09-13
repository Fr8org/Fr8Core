using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using PluginBase.BaseClasses;
using System.Collections.Generic;
using Data.States;

namespace pluginDocuSign.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionTemplateController : ApiController
    {
        [HttpGet]
        [Route("action_templates")]
        public IHttpActionResult Get()
        {
            var actionTemplate = new ActionTemplateDO()
            {
                Plugin = new PluginDO { Name = "localhost:53234", BaseEndPoint = "localhost:53234", PluginStatus = PluginStatus.Active },
                Version = "1.0",
                Name = "Wait For DocuSign Event",
                ActionProcessor = "DockyardAzureDocuSignService" 
            };

            var actionList = new List<ActionTemplateDO>()
            {
                actionTemplate
            };

            return Ok(actionList);
        }
    }
}