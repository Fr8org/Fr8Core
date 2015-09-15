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
            var waitForDocusignEventActionTemplate = new ActivityTemplateDO()
            {
                Plugin = new PluginDO { Name = "localhost:53234", Endpoint = "localhost:53234", PluginStatus = PluginStatus.Active },
                Version = "1.0",
                Name = "Wait For DocuSign Event"
            };

            var extractDataFromEnvelopeActionTemplate = new ActivityTemplateDO()
            {
                Plugin = new PluginDO { Name = "localhost:53234", Endpoint = "localhost:53234", PluginStatus = PluginStatus.Active },
                Version = "1.0",
                Name = "Extract Data From DocuSign Envelopes"
            };

            var actionList = new List<ActivityTemplateDO>()
            {
                waitForDocusignEventActionTemplate,
                extractDataFromEnvelopeActionTemplate
            };

            return Ok(actionList);
        }
    }
}