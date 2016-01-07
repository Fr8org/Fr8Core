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
                Version = "1",
                AuthenticationType = AuthenticationType.Internal
            };

            var waitForDocusignEventActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Monitor_DocuSign_Envelope_Activity",
                Label = "Monitor DocuSign Envelope Activity",
                Category = ActivityCategory.Monitors,
                Terminal = terminal,
                NeedsAuthentication = true,
                MinPaneWidth = 330,
                Help = new Data.Control.HelpControlDTO("Monitor_DocuSign_Envelope_Activity_SampleHelp1", "MenuItem")
            };

            var sendDocuSignEnvelopeActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Send_DocuSign_Envelope",
                Label = "Send DocuSign Envelope",
                Category = ActivityCategory.Forwarders,
                Terminal = terminal,
                NeedsAuthentication = true,
                MinPaneWidth = 330
            };

            var extractDataFromEnvelopeActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Get_DocuSign_Envelope",
                Label = "Get DocuSign Envelope",
                Category = ActivityCategory.Receivers,
                Terminal = terminal,
                NeedsAuthentication = true,
                MinPaneWidth = 330
            };

            var getDocuSignTemplateActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Get_DocuSign_Template",
                Label = "Get DocuSign Template",
                Category = ActivityCategory.Receivers,
                Terminal = terminal,
                NeedsAuthentication = true,
                MinPaneWidth = 330
            };

            var recordDocuSignEvents = new ActivityTemplateDTO
            {
                Name = "Record_DocuSign_Events",
                Label = "Record DocuSign Events",
                Version = "1",
                Category = ActivityCategory.Forwarders,
                Terminal = terminal,
                NeedsAuthentication = true,
                MinPaneWidth = 330
            };

            var mailMergeActionTemplate = new ActivityTemplateDTO
            {
                Name = "Mail_Merge_Into_DocuSign",
                Label = "Mail Merge Into DocuSign",
                Version = "1",
                NeedsAuthentication = true,
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
                NeedsAuthentication = true,
                Terminal = terminal,
                MinPaneWidth = 380
            };
            
            
            var queryDocusign = new ActivityTemplateDTO
            {
                Name = "Query_DocuSign",
                Label = "Query DocuSign",
                Version = "1",
                Category = ActivityCategory.Processors,
                NeedsAuthentication = true,
                Terminal = terminal,
                MinPaneWidth = 380
            };

            var generateDocusignReport = new ActivityTemplateDTO
            {
                Name = "Generate_DocuSign_Report",
                Label = "Generate a DocuSign Report",
                Version = "1",
                Category = ActivityCategory.Processors,
                NeedsAuthentication = true,
                Terminal = terminal,
                MinPaneWidth = 380
            };

            var searchDocusignHistory = new ActivityTemplateDTO
            {
                Name = "Find_DocuSign_Envelopes",
                Label = "Find DocuSign Envelopes",
                Version = "1",
                Category = ActivityCategory.Processors,
                NeedsAuthentication = true,
                Terminal = terminal,
                MinPaneWidth = 380
            };

            var showReport = new ActivityTemplateDTO
            {
                Name = "Show_Report",
                Label = "Show Report Onscreen",
                Version = "1",
                Category = ActivityCategory.Processors,
                NeedsAuthentication = false,
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
                generateDocusignReport,
                searchDocusignHistory,
                getDocuSignTemplateActionTemplate,
                showReport
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