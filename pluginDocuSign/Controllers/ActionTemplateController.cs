using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using PluginBase.BaseClasses;
using System.Collections.Generic;

namespace pluginDocuSign.Controllers
{    
    [RoutePrefix("action_templates")]
    public class ActionTemplateController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            var actionTemplate = new ActionTemplateDTO()
            {
                DefaultEndPoint = "localhost:53234",
                Version = "1.0",
                Name = "Wait For DocuSign Event"
            };

            var actionList = new List<ActionTemplateDTO>()
            {
                actionTemplate
            };

            return Ok(actionList);
        }
    }
}