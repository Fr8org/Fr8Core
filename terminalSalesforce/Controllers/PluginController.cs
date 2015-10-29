using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using TerminalBase.BaseClasses;
using System.Collections.Generic;
using Data.States;
using Utilities.Configuration.Azure;

namespace terminalSalesforce.Controllers
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
                Name = "terminalSalesforce",
                PluginStatus = PluginStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                Version = "1"
            };

            var action = new ActivityTemplateDO()
            {
                Version = "1",
                Name = "Create_Lead",
                Label = "Create Lead",
                Plugin = plugin,
                AuthenticationType = AuthenticationType.External,
                Category = ActivityCategory.Forwarders,
				MinPaneWidth = 330
            };

            var actionList = new List<ActivityTemplateDO>()
            {
                action
            };

            return Ok(actionList);
        }
    }
}