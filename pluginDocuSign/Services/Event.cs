using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Managers;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Data.States;
using Newtonsoft.Json;
using pluginDocuSign.Infrastructure;
using StructureMap;
using Core.Interfaces;
using Utilities.Configuration.Azure;

namespace pluginDocuSign.Services
{
	public class Event : pluginDocuSign.Interfaces.IEvent
    {
        private readonly EventReporter _alertReporter;
        private readonly ICrateManager _crate;

        public Event()
        {
            _alertReporter = ObjectFactory.GetInstance<EventReporter>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        public async Task<object> Process(string curExternalEventPayload)
        {
            //parse the external event xml payload
            List<DocuSignEventDO> curExternalEvents;
            string curEnvelopeId;
            Parse(curExternalEventPayload, out curExternalEvents, out curEnvelopeId);

            //prepare the content from the external event payload
            var curDocuSignEnvelopeInfo = DocuSignConnectParser.GetEnvelopeInformation(curExternalEventPayload);
            var eventReportContent = new EventReportCM
            {
                EventNames = "Envelope" + curDocuSignEnvelopeInfo.EnvelopeStatus.Status,
                ProcessDOId = "",
                EventPayload = ExtractEventPayload(curExternalEvents).ToList(),
                ExternalAccountId = curDocuSignEnvelopeInfo.EnvelopeStatus.ExternalAccountId
            };

            //prepare the event report
            CrateDTO curEventReport = ObjectFactory.GetInstance<ICrateManager>()
                .Create("Standard Event Report", JsonConvert.SerializeObject(eventReportContent), "Standard Event Report", 7);

            string url = Regex.Match(CloudConfigurationManager.GetSetting("EventWebServerUrl"), @"(\w+://\w+:\d+)").Value + "/dockyard_events";
            var response = await new HttpClient().PostAsJsonAsync(new Uri(url, UriKind.Absolute), curEventReport);

            var content = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    return JsonConvert.DeserializeObject(content);
                }
                catch 
                {
                    return new
                    {
                        title = "Unexpected error while serving your request",
                        exception = new
                        {
                            Message = "Unexpected response from hub"
                        }
                    };
                }
            }

            return content;
        }

        private void Parse(string xmlPayload, out List<DocuSignEventDO> curEvents, out string curEnvelopeId)
        {
            curEvents = new List<DocuSignEventDO>();
            try
            {
                var docuSignEnvelopeInformation = DocuSignConnectParser.GetEnvelopeInformation(xmlPayload);
                curEnvelopeId = docuSignEnvelopeInformation.EnvelopeStatus.EnvelopeId;
                curEvents.Add(new DocuSignEventDO
                {
                    ExternalEventType = DocuSignEventNames.MapEnvelopeExternalEventType(docuSignEnvelopeInformation.EnvelopeStatus.Status),
                    EnvelopeId = docuSignEnvelopeInformation.EnvelopeStatus.EnvelopeId,
                    RecipientId = docuSignEnvelopeInformation.EnvelopeStatus.RecipientStatuses.Statuses[0].Id,
                    DocuSignObject = "Envelope",
                    Status = docuSignEnvelopeInformation.EnvelopeStatus.Status,
                    CreateDate = docuSignEnvelopeInformation.EnvelopeStatus.CreatedDate,
                    SentDate = docuSignEnvelopeInformation.EnvelopeStatus.SentDate,
                    DeliveredDate = docuSignEnvelopeInformation.EnvelopeStatus.DeliveredDate,
                    CompletedDate = docuSignEnvelopeInformation.EnvelopeStatus.CompletedDate,
                    Email = docuSignEnvelopeInformation.EnvelopeStatus.ExternalAccountId,
                    EventId = DocuSignEventNames.MapEnvelopeExternalEventType(docuSignEnvelopeInformation.EnvelopeStatus.Status).ToString()
                });
            }
            catch (ArgumentException)
            {
                _alertReporter.ImproperDocusignNotificationReceived(
                    "Cannot extract envelopeId from DocuSign notification: UserId {0}, XML Payload\r\n{1}");
                throw new ArgumentException();
            }
        }

        private IEnumerable<CrateDTO> ExtractEventPayload(IEnumerable<DocuSignEventDO> curEvents)
        {
            IList<CrateDTO> curEventPayloadData = new List<CrateDTO>();

            foreach (var curEvent in curEvents)
            {
               var payloadCrate= _crate.CreatePayloadDataCrate(CreateKeyValuePairList(curEvent));
               curEventPayloadData.Add(payloadCrate);
            }
                   
            return curEventPayloadData;
        }

        private List<KeyValuePair<string,string>> CreateKeyValuePairList(DocuSignEventDO curEvent)
        {
            List<KeyValuePair<string, string>> returnList = new List<KeyValuePair<string, string>>();
            returnList.Add(new KeyValuePair<string,string>("EnvelopeId",curEvent.EnvelopeId));
            returnList.Add(new KeyValuePair<string,string>("ExternalEventType",curEvent.ExternalEventType.ToString()));
            returnList.Add(new KeyValuePair<string,string>("RecipientId",curEvent.RecipientId));

            returnList.Add(new KeyValuePair<string, string>("Object", curEvent.DocuSignObject));
            returnList.Add(new KeyValuePair<string, string>("Status", curEvent.Status));
            returnList.Add(new KeyValuePair<string, string>("CreateDate", curEvent.CreateDate));
            returnList.Add(new KeyValuePair<string, string>("SentDate", curEvent.SentDate));

            returnList.Add(new KeyValuePair<string, string>("DeliveredDate", curEvent.DeliveredDate));
            returnList.Add(new KeyValuePair<string, string>("CompletedDate", curEvent.CompletedDate));
            returnList.Add(new KeyValuePair<string, string>("Email", curEvent.Email));
            returnList.Add(new KeyValuePair<string, string>("EventId", curEvent.EventId));

            return returnList;
            }


    }
}