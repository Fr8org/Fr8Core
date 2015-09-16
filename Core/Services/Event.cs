
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Core.Interfaces;
using Core.Managers.APIManagers.Transmitters.Restful;
using Core.Utilities;
using Data.Entities.DocuSignParserModels;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using StructureMap;

namespace Core.Services
{
    /// <summary>
    /// Event service implementation
    /// </summary>
    public class Event : IEvent
    {
        /// <see cref="IEvent.HandlePluginIncident"/>
        public void HandlePluginIncident(LoggingData incident)
        {
            EventManager.ReportPluginIncident(incident);
        }

        public void HandlePluginEvent(LoggingData eventData)
        {
            EventManager.ReportPluginEvent(eventData);
        }

        public void Process(string curPluginName, string curExternalEventPayload)
        {
            switch (curPluginName)
            {
                case "pluginAzureSqlServer":
                    ProcessAzureSqlServerExternalEvent(curExternalEventPayload);
                    break;
                case "pluginDocuSign":
                    ProcessDocuSignExternalEvent(curExternalEventPayload);
                    break;
                case "pluginSlack":
                    ProcessSlackExternalEvent(curExternalEventPayload);
                    break;
                case "pluginDockyardCore":
                    ProcessDockyardCoreExternalEvent(curExternalEventPayload);
                    break;
                default:
                    throw new InvalidOperationException("Request made from unknown plugin to process the external event");
            }
        }

        private void ProcessDocuSignExternalEvent(string curExternalEventPayload)
        {
             //get the docusing envelope info
            var curDocuSignEnvelopeInfo = DocuSignConnectParser.GetEnvelopeInformation(curExternalEventPayload);

            //prepare the content from the external event payload
            var eventReportContent = new EventReportMS
            {
                EventNames = "DocuSign Envelope " + curDocuSignEnvelopeInfo.EnvelopeStatus.Status,
                ProcessDOId = "",
                EventPayload = new List<CrateDTO>
                {
                    new CrateDTO
                    {
                        //here we need to process the incoming xml payload to extract the important fields.
                    }
                }
            };

            //prepare the event report
            CrateDTO curEventReport = ObjectFactory.GetInstance<ICrate>()
                .Create("Standard Event Report", JsonConvert.SerializeObject(eventReportContent), "Standard Event Report", 7);

            string url = Regex.Match(ConfigurationManager.AppSettings["EventWebServerUrl"], @"(\w+://\w+:\d+)").Value + "/dockyard_events";
            new HttpClient().PostAsJsonAsync(new Uri(url, UriKind.Absolute), curEventReport);
        }

        private void ProcessAzureSqlServerExternalEvent(string curExternalEventPayload)
        {
            //Process external event payload from Azure Sql Server plugin
        }

        private void ProcessDockyardCoreExternalEvent(string curExternalEventPayload)
        {
            //Process external event payload from Azure Sql Server plugin
        }

        private void ProcessSlackExternalEvent(string curExternalEventPayload)
        {
            //Process external event payload from Slack plugin
        }
    }
}
