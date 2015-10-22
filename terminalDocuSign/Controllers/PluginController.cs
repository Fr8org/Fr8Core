using System;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Entities;
using Newtonsoft.Json;
using System.Reflection;
using TerminalBase.BaseClasses;
using System.Collections.Generic;
using Core.Services;
using Data.States;
using Utilities.Configuration.Azure;

namespace terminalDocuSign.Controllers
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
                Name = "terminalDocuSign",
                PluginStatus = PluginStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                RequiresAuthentication = true,
                Version = "1"
            };

            var waitForDocusignEventActionTemplate = new ActivityTemplateDO()
            {
                Version = "1",
                Name = "Monitor_DocuSign",
                Label = "Monitor DocuSign",
                Category = ActivityCategory.Monitors,
                Plugin = plugin,
                MinPaneWidth = 330
            };

			var sendDocuSignEnvelopeActionTemplate = new ActivityTemplateDO()
			{
				Version = "1",
				Name = "Send_DocuSign_Envelope",
                Label = "Send DocuSign Envelope",
                Category = ActivityCategory.Forwarders,
				Plugin = plugin,
                MinPaneWidth = 330
			};

            var extractDataFromEnvelopeActionTemplate = new ActivityTemplateDO()
            {
                Version = "1",
                Name = "Receive_DocuSign_Envelope",
                Label = "Receive DocuSign Envelope",
                Category = ActivityCategory.Receivers,
                Plugin = plugin,
                MinPaneWidth = 330
            };

            var recordDocuSignEvents = new ActivityTemplateDO
            {
                Name = "Record_DocuSign_Events",
                Label = "Record DocuSign Events",
                Version = "1",
                Category = ActivityCategory.Forwarders,
                Plugin = plugin,
                MinPaneWidth = 330
            };

            var collectFormDataSolution = new ActivityTemplateDO
            {
                Name = "Collect_Form_Data_Solution",
                Label = "Collect Form Data Solution",
                Version = "1",
                Category = ActivityCategory.Solution,
                Plugin = plugin,
                MinPaneWidth = 330
            };

            var actionList = new List<ActivityTemplateDO>()
            {
                waitForDocusignEventActionTemplate,
                extractDataFromEnvelopeActionTemplate,
                sendDocuSignEnvelopeActionTemplate,
                recordDocuSignEvents,
                collectFormDataSolution
            };

            return Ok(actionList);
        }
    }
}