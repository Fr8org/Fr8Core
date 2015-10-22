using Core.Interfaces;
using Core.Managers;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using terminalSalesforce.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Configuration;
using System.Net.Http;
using System.Text.RegularExpressions;
using Data.Interfaces.ManifestSchemas;
using Core.StructureMap;
using TerminalBase.BaseClasses;

namespace terminalSalesforce.Services
{
    public class Event : terminalSalesforce.Infrastructure.IEvent
    {
       
        private readonly ICrateManager _crate;        
        private BasePluginController _basePluginController = new BasePluginController();
       

        public Event()
        {
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }


        public CrateDTO ProcessEvent(string curExternalEventPayload)
        {
            string leadId = string.Empty;
            string accountId = string.Empty;
            Parse(curExternalEventPayload, out leadId, out accountId);

            //prepare the content from the external event payload            
            var eventReportContent = new EventReportCM
            {
                EventNames = "Lead Created",
                ContainerDoId = "",
                EventPayload = ExtractEventPayload(leadId,accountId).ToList(),
                ExternalAccountId = accountId,
                Source = "Salesforce"
            };

            CrateDTO curEventReport = ObjectFactory.GetInstance<ICrateManager>()
                .Create("Lead Created", JsonConvert.SerializeObject(eventReportContent), "Standard Event Report", 7);

            return curEventReport;
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
                _basePluginController.ReportPluginError("pluginSalesforce", e);               
                throw new ArgumentException();
            }
        }

        private IEnumerable<CrateDTO> ExtractEventPayload(string leadId, string accountId)
        {
            IList<CrateDTO> curEventPayloadData = new List<CrateDTO>();
            var payLoadData = _crate.CreatePayloadDataCrate(CreateKeyValuePairList(leadId, accountId));
            curEventPayloadData.Add(payLoadData);
            return curEventPayloadData;
        }

        private List<KeyValuePair<string,string>> CreateKeyValuePairList(string leadId,string accountId)
        {
            List<KeyValuePair<string, string>> returnList = new List<KeyValuePair<string, string>>();
            returnList.Add(new KeyValuePair<string,string>("LeadID",leadId));
            returnList.Add(new KeyValuePair<string,string>("AccountID",accountId));
            return returnList;
        }
    }
}