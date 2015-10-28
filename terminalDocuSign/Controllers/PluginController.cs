using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using AutoMapper;
using Newtonsoft.Json;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Services;
using TerminalBase.BaseClasses;
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
                Version = "1"
            };

            var waitForDocusignEventActionTemplate = new ActivityTemplateDO()
            {
                Version = "1",
                Name = "Monitor_DocuSign",
                Label = "Monitor DocuSign",
                Category = ActivityCategory.Monitors,
                Plugin = plugin,
                AuthenticationType = AuthenticationType.Internal,
				MinPaneWidth = 330
            };

			var sendDocuSignEnvelopeActionTemplate = new ActivityTemplateDO()
			{
				Version = "1",
				Name = "Send_DocuSign_Envelope",
                Label = "Send DocuSign Envelope",
                Category = ActivityCategory.Forwarders,
				Plugin = plugin,
                AuthenticationType = AuthenticationType.Internal,
				MinPaneWidth = 330
			};

            var extractDataFromEnvelopeActionTemplate = new ActivityTemplateDO()
            {
                Version = "1",
                Name = "Receive_DocuSign_Envelope",
                Label = "Receive DocuSign Envelope",
                Category = ActivityCategory.Receivers,
                Plugin = plugin,
                AuthenticationType = AuthenticationType.Internal,
				MinPaneWidth = 330
            };

            var recordDocuSignEvents = new ActivityTemplateDO
            {
                Name = "Record_DocuSign_Events",
                Label = "Record DocuSign Events",
                Version = "1",
                Category = ActivityCategory.Forwarders,
                Plugin = plugin,
                AuthenticationType = AuthenticationType.Internal,
				MinPaneWidth = 330
            };

            var mailMergeActionTemplate = new ActivityTemplateDO
            {
                Name = "Mail_Merge_Into_DocuSign",
                Label = "Mail Merge Into DocuSign",
                Version = "1",
                AuthenticationType = AuthenticationType.Internal,
                Category = ActivityCategory.Solution,
                Plugin = plugin,
                MinPaneWidth = 500
            };

            var actionList = new List<ActivityTemplateDO>()
            {
                waitForDocusignEventActionTemplate,
                extractDataFromEnvelopeActionTemplate,
                sendDocuSignEnvelopeActionTemplate,
                recordDocuSignEvents,
                mailMergeActionTemplate
            };

            return Ok(actionList);
        }
    }
}