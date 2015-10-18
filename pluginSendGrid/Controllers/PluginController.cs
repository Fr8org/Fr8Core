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
namespace pluginSalesforce.Controllers
{
    [RoutePrefix("plugins")]
    public class PluginController : ApiController
    {
        [HttpGet]
        [Route("discover")]
        public IHttpActionResult Get()
        {
            var plugin = new PluginDO()
            {
                Name = "pluginSendGrid",
                PluginStatus = PluginStatus.Active,
                Endpoint = "localhost:10601",
                Version = "1"
            };

            var action = new ActivityTemplateDO()
            {
                Name = "SendEmailViaSendGrid",
                Label = "Send Email Vie Send Grid",
                Version = "1",
                Plugin = plugin,
                Category = ActivityCategory.Forwarders
            };

            var actionList = new List<ActivityTemplateDO>()
            {
                action
            };

            return Ok(actionList);
        }
    }
}