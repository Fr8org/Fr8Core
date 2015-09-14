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
            var waitForDocusignEventActionTemplate = new ActivityTemplateDTO()
            {
                DefaultEndPoint = "localhost:53234",
                Version = "1.0",
                Name = "Wait For DocuSign Event"
            };

            var extractDataFromEnvelopeActionTemplate = new ActivityTemplateDTO()
            {
                DefaultEndPoint = "localhost:53234",
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