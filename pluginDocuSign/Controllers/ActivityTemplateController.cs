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
    public class ActivityTemplateController : ApiController
    {
        [HttpGet]
        [Route("action_templates")]
        public IHttpActionResult Get()
        {
            var waitForDocusignEventActionTemplate = new ActivityTemplateDTO()
            {
               
                Version = "1.0",
                Name = "Wait For DocuSign Event"

            };

            var extractDataFromEnvelopeActionTemplate = new ActivityTemplateDTO()
            {
               
                Version = "1.0",
                Name = "Extract Data From DocuSign Envelopes"
            };

            var actionList = new List<ActivityTemplateDTO>()
            {
                waitForDocusignEventActionTemplate,
                extractDataFromEnvelopeActionTemplate
            };

            return Ok(actionList);
        }
    }
}