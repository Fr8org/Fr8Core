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
using System.Web.Http.Description;
using Data.Interfaces.Manifests;

namespace terminalSalesforce.Controllers
{
     [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult Get()
        {
            var plugin = new TerminalDO()
            {
                Name = "terminalSalesforce",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                Version = "1"
            };

            var createLeadAction = new ActivityTemplateDO()
            {
                Version = "1",
                Name = "Create_Lead",
                Label = "Create Lead",
                Terminal = plugin,
                AuthenticationType = AuthenticationType.External,
                Category = ActivityCategory.Forwarders,
                MinPaneWidth = 330
            };

            var createContactAction = new ActivityTemplateDO()
            {
                Version = "1",
                Name = "Create_Contact",
                Label = "Create Contact",
                Terminal = plugin,
                AuthenticationType = AuthenticationType.External,
                Category = ActivityCategory.Forwarders,
                MinPaneWidth = 330
            };

            var createAccountAction = new ActivityTemplateDO()
            {
                Version = "1",
                Name = "Create_Account",
                Label = "Create Account",
                Terminal = plugin,
                AuthenticationType = AuthenticationType.External,
                Category = ActivityCategory.Forwarders,
                MinPaneWidth = 330
            };

            var actionList = new List<ActivityTemplateDO>()
            {
                createLeadAction,createContactAction,createAccountAction
            };

            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = plugin,
                Actions = actionList
            };
            return Json(curStandardFr8TerminalCM);
        }
    }
}