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
using System.Web.Http.Description;
using Data.Interfaces.Manifests;

namespace terminalDocuSign.Controllers
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
                Name = "terminalDocuSign",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("TerminalEndpoint"),
                Version = "1"
            };

            var waitForDocusignEventActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Monitor_DocuSign_Envelope_Activity",
                Label = "Monitor DocuSign Envelope Activity",
                Category = ActivityCategory.Monitors,
                Terminal = terminal,
                AuthenticationType = AuthenticationType.Internal,
                MinPaneWidth = 330
            };

            var sendDocuSignEnvelopeActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Send_DocuSign_Envelope",
                Label = "Send DocuSign Envelope",
                Category = ActivityCategory.Forwarders,
                Terminal = terminal,
                AuthenticationType = AuthenticationType.Internal,
                MinPaneWidth = 330
            };

            var extractDataFromEnvelopeActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Receive_DocuSign_Envelope",
                Label = "Receive DocuSign Envelope",
                Category = ActivityCategory.Receivers,
                Terminal = terminal,
                AuthenticationType = AuthenticationType.Internal,
                MinPaneWidth = 330
            };

            var recordDocuSignEvents = new ActivityTemplateDTO
            {
                Name = "Record_DocuSign_Events",
                Label = "Record DocuSign Events",
                Version = "1",
                Category = ActivityCategory.Forwarders,
                Terminal = terminal,
                AuthenticationType = AuthenticationType.Internal,
                MinPaneWidth = 330
            };

            var mailMergeActionTemplate = new ActivityTemplateDTO
            {
                Name = "Mail_Merge_Into_DocuSign",
                Label = "Mail Merge Into DocuSign",
                Version = "1",
                AuthenticationType = AuthenticationType.Internal,
                Category = ActivityCategory.Solution,
                Terminal = terminal,
                MinPaneWidth = 500
            };

            var collectFormDataSolution = new ActivityTemplateDTO
            {
				Name = "Extract_Data_From_Envelopes",
				Label = "Extract Data From Envelopes",
                Version = "1",
                Category = ActivityCategory.Solution,
                Terminal = terminal,
                MinPaneWidth = 380
            };

            var richDocumentNotificationsSolution = new ActivityTemplateDTO
            {
                Name = "Rich_Document_Notifications",
                Label = "Rich Document Notifications",
                Version = "1",
                Category = ActivityCategory.Solution,
                AuthenticationType = AuthenticationType.Internal,
                Terminal = terminal,
                MinPaneWidth = 380
            };
            
            
            var queryDocusign = new ActivityTemplateDTO
            {
                Name = "Query_DocuSign",
                Label = "Query DocuSign",
                Version = "1",
                Category = ActivityCategory.Processors,
                AuthenticationType = AuthenticationType.Internal,
                Terminal = terminal,
                MinPaneWidth = 380
            };

            var searchDocusignHistory = new ActivityTemplateDTO
            {
                Name = "Search_DocuSign_History",
                Label = "Search DocuSign History",
                Version = "1",
                Category = ActivityCategory.Processors,
                AuthenticationType = AuthenticationType.Internal,
                Terminal = terminal,
                MinPaneWidth = 380
            };


            var actionList = new List<ActivityTemplateDTO>()
            {
                waitForDocusignEventActionTemplate,
                extractDataFromEnvelopeActionTemplate,
                sendDocuSignEnvelopeActionTemplate,
                recordDocuSignEvents,
                mailMergeActionTemplate,
                collectFormDataSolution,
                richDocumentNotificationsSolution,
                queryDocusign,
                searchDocusignHistory
            };

            var curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Actions = actionList
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}