using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data.Crates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Interfaces;
using Utilities.Configuration.Azure;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Services
{
    public class Event : terminalDocuSign.Interfaces.IEvent
    {
        private readonly EventReporter _alertReporter;
        private readonly ICrateManager _crate;
        private readonly IDocuSignRoute _docuSignRoute;

        public Event()
        {
            _alertReporter = ObjectFactory.GetInstance<EventReporter>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _docuSignRoute = ObjectFactory.GetInstance<IDocuSignRoute>();
        }

        public async Task<object> Process(string curExternalEventPayload)
        {
            //DO - 1449
            //if the event payload is Fr8 User ID, it is DocuSign Authentication Completed event
            //The Monitor All DocuSign Events route should be creaed in this case.
            if (curExternalEventPayload.Contains("fr8_user_id"))
            {
                var curFr8UserId = JObject.Parse(curExternalEventPayload)["fr8_user_id"].Value<string>();

                if (string.IsNullOrEmpty(curFr8UserId))
                {
                    throw new ArgumentException("Fr8 User ID is not in the correct format.");
                }

                //create MonitorAllDocuSignEvents route
                await _docuSignRoute.CreateRoute_MonitorAllDocuSignEvents(curFr8UserId);

                return null;
            } 

            //parse the external event xml payload
            List<DocuSignEventDTO> curExternalEvents;
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
            var curEventReport = Crate.FromContent("Standard Event Report", eventReportContent);

            string url = Regex.Match(CloudConfigurationManager.GetSetting("EventWebServerUrl"), @"(\w+://\w+:\d+)").Value + "/fr8_events";
            var response = await new HttpClient().PostAsJsonAsync(new Uri(url, UriKind.Absolute), _crate.ToDto(curEventReport));

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

        private void Parse(string xmlPayload, out List<DocuSignEventDTO> curEvents, out string curEnvelopeId)
        {
            curEvents = new List<DocuSignEventDTO>();
            try
            {
                var docuSignEnvelopeInformation = DocuSignConnectParser.GetEnvelopeInformation(xmlPayload);
                curEnvelopeId = docuSignEnvelopeInformation.EnvelopeStatus.EnvelopeId;
                curEvents.Add(new DocuSignEventDTO
                {
                    ExternalEventType = DocuSignEventNames.MapEnvelopeExternalEventType(docuSignEnvelopeInformation.EnvelopeStatus.Status),
                    EnvelopeId = docuSignEnvelopeInformation.EnvelopeStatus.EnvelopeId,
                    DocumentName = docuSignEnvelopeInformation.EnvelopeStatus.DocumentStatuses.Statuses[0].Name,
                    TemplateName = docuSignEnvelopeInformation.EnvelopeStatus.DocumentStatuses.Statuses[0].TemplateName,
                    RecipientId = docuSignEnvelopeInformation.EnvelopeStatus.RecipientStatuses.Statuses[0].Id,
                    RecipientEmail = docuSignEnvelopeInformation.EnvelopeStatus.RecipientStatuses.Statuses[0].Email,
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

        private CrateStorage ExtractEventPayload(IEnumerable<DocuSignEventDTO> curEvents)
        {
            var stroage = new CrateStorage();
            
            foreach (var curEvent in curEvents)
            {
                var payloadCrate = Data.Crates.Crate.FromContent("", new StandardPayloadDataCM(CreateKeyValuePairList(curEvent)));
                stroage.Add(payloadCrate);
            }

            return stroage;
        }

        private List<FieldDTO> CreateKeyValuePairList(DocuSignEventDTO curEvent)
        {
            List<FieldDTO> returnList = new List<FieldDTO>();
            returnList.Add(new FieldDTO("EnvelopeId",curEvent.EnvelopeId));
            returnList.Add(new FieldDTO("ExternalEventType",curEvent.ExternalEventType.ToString()));
            returnList.Add(new FieldDTO("RecipientId",curEvent.RecipientId));
            returnList.Add(new FieldDTO("RecipientEmail", curEvent.RecipientEmail));

            returnList.Add(new FieldDTO("DocumentName", curEvent.DocumentName));
            returnList.Add(new FieldDTO("TemplateName", curEvent.TemplateName));

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