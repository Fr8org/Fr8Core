using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text.RegularExpressions;
using Core.Interfaces;
using Core.Utilities;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using StructureMap;

namespace pluginDocuSign.Services
{
    public class Event : IEvent
    {
        public void Process(string curExternalEventPayload)
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
    }
}