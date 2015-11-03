using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data.Crates;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Utilities.Configuration.Azure;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Services
{
    public class Event : Interfaces.IEvent
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
                ContainerDoId = "",
                EventPayload = ExtractEventPayload(curExternalEvents),
                ExternalAccountId = curDocuSignEnvelopeInfo.EnvelopeStatus.ExternalAccountId
            };

            //prepare the event report
            var curEventReport = _crate.CreateStandardEventReportCrate("Standard Event Report", eventReportContent);

            string url = Regex.Match(CloudConfigurationManager.GetSetting("EventWebServerUrl"), @"(\w+://\w+:\d+)").Value + "/dockyard_events";
            var response = await new HttpClient().PostAsJsonAsync(new Uri(url, UriKind.Absolute), _crate.SerializeToProxy(curEventReport));

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

        private CrateStorage ExtractEventPayload(IEnumerable<DocuSignEventDO> curEvents)
        {
            var storage = new CrateStorage();

            foreach (var curEvent in curEvents)
            {
                storage.Add(Crate.FromContent("", new StandardPayloadDataCM(CreateKeyValuePairList(curEvent))));
            }

            return storage;
        }

        private List<FieldDTO> CreateKeyValuePairList(DocuSignEventDO curEvent)
        {
            List<FieldDTO> returnList = new List<FieldDTO>();
            returnList.Add(new FieldDTO("EnvelopeId", curEvent.EnvelopeId));
            returnList.Add(new FieldDTO("ExternalEventType", curEvent.ExternalEventType.ToString()));
            returnList.Add(new FieldDTO("RecipientId", curEvent.RecipientId));

            returnList.Add(new FieldDTO("Object", curEvent.DocuSignObject));
            returnList.Add(new FieldDTO("Status", curEvent.Status));
            returnList.Add(new FieldDTO("CreateDate", curEvent.CreateDate));
            returnList.Add(new FieldDTO("SentDate", curEvent.SentDate));

            returnList.Add(new FieldDTO("DeliveredDate", curEvent.DeliveredDate));
            returnList.Add(new FieldDTO("CompletedDate", curEvent.CompletedDate));
            returnList.Add(new FieldDTO("Email", curEvent.Email));
            returnList.Add(new FieldDTO("EventId", curEvent.EventId));

            return returnList;
        }
    }
}