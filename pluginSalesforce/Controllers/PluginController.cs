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
                Name = "pluginSalesforce",
                PluginStatus = PluginStatus.Active,
                Endpoint = "localhost:51234",
                Version = "1"
            };

            var action = new ActivityTemplateDO()
            {
                Version = "1",
                Name = "Create_Lead",
                Plugin = plugin
            };

            var actionList = new List<ActivityTemplateDO>()
            {
                action
            };

            return Ok(actionList);
        }
    }
}