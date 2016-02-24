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
            var terminal = new TerminalDTO()
            {
                Name = "terminalSalesforce",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                Version = "1",
                AuthenticationType = AuthenticationType.External
            };

	        var webService = new WebServiceDTO
	        {
				Name = "Salesforce"
	        };

            var createLeadAction = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Create_Lead",
                Label = "Create Lead",
                Terminal = terminal,
                NeedsAuthentication = true,
                Category = ActivityCategory.Forwarders,
                MinPaneWidth = 330,
                WebService = webService
            };

            var createContactAction = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Create_Contact",
                Label = "Create Contact",
                Terminal = terminal,
                NeedsAuthentication = true,
                Category = ActivityCategory.Forwarders,
                MinPaneWidth = 330,
                WebService = webService
            };

            var createAccountAction = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Create_Account",
                Label = "Create Account",
                Terminal = terminal,
                NeedsAuthentication = true,
                Category = ActivityCategory.Forwarders,
                MinPaneWidth = 330,
                WebService = webService
            };

            var getDataAction = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Get_Data",
                Label = "Get Data from Salesforce.com",
                Terminal = terminal,
                NeedsAuthentication = true,
                Category = ActivityCategory.Receivers,
                MinPaneWidth = 330,
                WebService = webService
            };

            var actionList = new List<ActivityTemplateDTO>()
            {
                createLeadAction,createContactAction,createAccountAction, getDataAction
            };

            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Activities = actionList
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}