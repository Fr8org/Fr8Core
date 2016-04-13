using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using StructureMap;
using terminalSalesforce.Infrastructure;
using TerminalBase.BaseClasses;
using System.Linq;

namespace terminalSalesforce.Services
{
    public class Event : IEvent
    {
       
        private readonly ICrateManager _crate;
        private BaseTerminalController _baseTerminalController = new BaseTerminalController();
       

        public Event()
        {
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }


        public Task<Crate> ProcessEvent(string curExternalEventPayload)
        {
            //string leadId = string.Empty;
            //string accountId = string.Empty;
            //Parse(curExternalEventPayload, out leadId, out accountId);

            try
            {
                var curEventEnvelope = SalesforceNotificationParser.GetEnvelopeInformation(curExternalEventPayload);

                string accountId = string.Empty;

                if(curEventEnvelope.Body.Notifications.NotificationList != null && curEventEnvelope.Body.Notifications.NotificationList.Count > 0)
                {
                    accountId = curEventEnvelope.Body.Notifications.NotificationList[0].SObject.OwnerId;
                }

                //prepare the content from the external event payload            
                var eventReportContent = new EventReportCM
                {
                    EventNames = GetEventNames(curEventEnvelope),
                    ContainerDoId = "",
                    EventPayload = ExtractEventPayload(curEventEnvelope),
                    ExternalAccountId = accountId,
                    Manufacturer = "Salesforce",
                };

                return Task.FromResult(Crate.FromContent("Standard Event Report", eventReportContent));
            }
            catch (Exception e)
            {
                _baseTerminalController.ReportTerminalError("terminalSalesforce", e);
                throw new Exception(string.Format("Error while processing. \r\n{0}", curExternalEventPayload));
            }
            
        }

        private string GetEventNames(Envelope curEventEnvelope)
        {
            List<string> result = new List<string>();

            result = curEventEnvelope.Body.Notifications.NotificationList.ToList().Select(notification => {
                            if(notification.SObject.CreatedDate.Equals(notification.SObject.LastModifiedDate))
                            {
                                return notification.SObject.Type.Substring(notification.SObject.Type.LastIndexOf(':') + 1) + "Created";
                            }
                            else
                            {
                                return notification.SObject.Type.Substring(notification.SObject.Type.LastIndexOf(':') + 1) + "Updated";
                            }
                        }).ToList();

            return string.Join(",", result);
        }

        private ICrateStorage ExtractEventPayload(Envelope curEventEnvelope)
        {
            var stroage = new CrateStorage();

            foreach (var curEvent in curEventEnvelope.Body.Notifications.NotificationList)
            {
                var payloadCrate = Data.Crates.Crate.FromContent("", new StandardPayloadDataCM(CreateKeyValuePairList(curEvent)));
                stroage.Add(payloadCrate);
            }

            return stroage;
        }

        private List<FieldDTO> CreateKeyValuePairList(Notification curNotification)
        {
            var returnList = new List<FieldDTO>();

            returnList.Add(new FieldDTO("Id", curNotification.SObject.Id));
            returnList.Add(new FieldDTO("CreatedDate", curNotification.SObject.CreatedDate.ToString()));
            returnList.Add(new FieldDTO("LastModifiedDate", curNotification.SObject.LastModifiedDate.ToString()));

            return returnList;
        }
    }
}