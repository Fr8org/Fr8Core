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
            string leadId = string.Empty;
            string accountId = string.Empty;
            Parse(curExternalEventPayload, out leadId, out accountId);

            //prepare the content from the external event payload            
            var eventReportContent = new EventReportCM
            {
                EventNames = "Lead Created",
                ContainerDoId = "",
                EventPayload = new CrateStorage(ExtractEventPayload(leadId, accountId)),
                ExternalAccountId = accountId,
                Source = "Salesforce"
            };

            return Task.FromResult(Crate.FromContent("Lead Created", eventReportContent));
        }

        public void Parse(string xmlPayload, out string leadId, out string accountId)
        {
            try
            {
                var documentObject = SalesforceNotificationParser.GetEnvelopeInformation(xmlPayload);
                leadId = string.Empty;
                accountId = string.Empty;
                if (documentObject.Body.Notifications.Notification.SObject!=null)
                {                   
                    leadId = documentObject.Body.Notifications.Notification.SObject.Id;
                    accountId = documentObject.Body.Notifications.Notification.SObject.OwnerId;
                }
                else
                {
                        throw new ArgumentException(String.Format("Cannot extract  Lead ID and Account ID from notification:, XML Payload\r\n{0}",xmlPayload));
                }
            }
            catch (ArgumentException e)
            {
                _baseTerminalController.ReportTerminalError("terminalSalesforce", e);               
                throw new ArgumentException();
            }
        }

        private IEnumerable<Crate> ExtractEventPayload(string leadId, string accountId)
        {
            IList<Crate> curEventPayloadData = new List<Crate>();

            var payLoadData = Crate.FromContent("", new StandardPayloadDataCM(CreateKeyValuePairList(leadId, accountId)));
            curEventPayloadData.Add(payLoadData);
            
            return curEventPayloadData;
        }

        private List<FieldDTO> CreateKeyValuePairList(string leadId,string accountId)
        {
            var returnList = new List<FieldDTO>();

            returnList.Add(new FieldDTO("LeadID", leadId));
            returnList.Add(new FieldDTO("AccountID", accountId));
            return returnList;
        }
    }
}