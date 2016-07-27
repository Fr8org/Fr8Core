using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using terminalSalesforce.Infrastructure;
using System.Linq;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Services;

namespace terminalSalesforce.Services
{
    public class Event : IEvent
    {
        private readonly IHubLoggerService _loggerService;

        public Event(IHubLoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        public async Task<Crate> ProcessEvent(string curExternalEventPayload)
        {
            try
            {
                var curEventEnvelope = SalesforceNotificationParser.GetEnvelopeInformation(curExternalEventPayload);

                string accountId = string.Empty;

                if(curEventEnvelope.Body.Notifications.NotificationList != null && curEventEnvelope.Body.Notifications.NotificationList.Length > 0)
                {
                    accountId = curEventEnvelope.Body.Notifications.NotificationList[0].SObject.OwnerId;
                }

                //prepare the content from the external event payload            
                var eventReportContent = new EventReportCM
                {
                    EventNames = GetEventNames(curEventEnvelope),
                    EventPayload = ExtractEventPayload(curEventEnvelope),
                    ExternalAccountId = accountId,
                    Manufacturer = "Salesforce",
                };

                return Crate.FromContent("Standard Event Report", eventReportContent);
            }
            catch (Exception e)
            {
                await _loggerService.ReportTerminalError(e);
                throw new Exception($"Error while processing. \r\n{curExternalEventPayload}");
            }
        }

        private string GetEventNames(Envelope curEventEnvelope)
        {
            if (curEventEnvelope?.Body?.Notifications == null)
            {
                return string.Empty;
            }

            var result = curEventEnvelope.Body.Notifications.NotificationList.ToList().Select(ExtractOccuredEvent).ToArray();

            return string.Join(",", result);
        }

        private ICrateStorage ExtractEventPayload(Envelope curEventEnvelope)
        {
            var stroage = new CrateStorage();

            var payloadDataCM = new StandardPayloadDataCM();
            foreach (var curNotification in curEventEnvelope.Body.Notifications.NotificationList)
            {
                payloadDataCM.PayloadObjects.Add(new PayloadObjectDTO(CreateKeyValuePairList(curNotification)));
            }

            var payloadCrate = Crate.FromContent("Salesforce Event Notification Payload", payloadDataCM);
            stroage.Add(payloadCrate);

            return stroage;
        }

        private List<KeyValueDTO> CreateKeyValuePairList(Notification curNotification)
        {
            var returnList = new List<KeyValueDTO>();

            returnList.Add(new KeyValueDTO("ObjectType", curNotification.SObject.Type.Substring(curNotification.SObject.Type.LastIndexOf(':') + 1)));
            returnList.Add(new KeyValueDTO("ObjectId", curNotification.SObject.Id));
            returnList.Add(new KeyValueDTO("CreatedDate", curNotification.SObject.CreatedDate.ToString()));
            returnList.Add(new KeyValueDTO("LastModifiedDate", curNotification.SObject.LastModifiedDate.ToString()));
            returnList.Add(new KeyValueDTO("OccuredEvent", ExtractOccuredEvent(curNotification)));


            return returnList;
        }

        private static string ExtractOccuredEvent(Notification notification)
        {
            if (notification.SObject.CreatedDate.Equals(notification.SObject.LastModifiedDate))
            {
                return notification.SObject.Type.Substring(notification.SObject.Type.LastIndexOf(':') + 1) + "Created";
            }
            else if (notification.SObject.CreatedDate < notification.SObject.LastModifiedDate)
            {
                return notification.SObject.Type.Substring(notification.SObject.Type.LastIndexOf(':') + 1) + "Updated";
            }
            else
            {
                throw new InvalidOperationException("Not able to detect the Salesforce event notification reason.");
            }
        }
    }
}