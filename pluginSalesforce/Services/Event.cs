using Core.Interfaces;
using Core.Managers;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using pluginSalesforce.Infrastructure;
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

namespace pluginSalesforce.Services
{
    public class Event : pluginSalesforce.Infrastructure.IEvent
    {
        private readonly EventReporter _alertReporter;
        private readonly ICrate _crate;
        private string eventWebServerUrl = string.Empty;

        public Event()
        {
            eventWebServerUrl = Regex.Match(ConfigurationManager.AppSettings["EventWebServerUrl"], @"(\w+://\w+:\d+)").Value + "/dockyard_events";
            _alertReporter = ObjectFactory.GetInstance<EventReporter>();
            _crate = ObjectFactory.GetInstance<ICrate>();
        }

        public void Process(string curExternalEventPayload)
        {
            string leadId = string.Empty;
            string accountId = string.Empty;
            Parse(curExternalEventPayload, out leadId, out accountId);

            //prepare the content from the external event payload            
            var eventReportContent = new EventReportMS
            {
                EventNames = "Lead Created",
                ProcessDOId = "",
                EventPayload = CreatePayLoadData(leadId, accountId).ToList(),
                ExternalAccountId = accountId
            };

            ////prepare the event report
            CrateDTO curEventReport = ObjectFactory.GetInstance<ICrate>()
                .Create("Lead Created", JsonConvert.SerializeObject(eventReportContent), "Standard Event Report", 7);

            new HttpClient().PostAsJsonAsync(new Uri(eventWebServerUrl, UriKind.Absolute), curEventReport);
        }

        public void Parse(string xmlPayload, out string leadId, out string accountId)
        {

            try
            {
                leadId = string.Empty;
                accountId = string.Empty;
                if (!String.IsNullOrEmpty(xmlPayload))
                {
                    XmlDocument xmlPayloadDoc = new XmlDocument();
                    xmlPayloadDoc.LoadXml(xmlPayload);
                    XmlNodeList notification = xmlPayloadDoc.GetElementsByTagName("sObject");
                    if (notification.Count != 0)
                    {
                        foreach (XmlNode node in notification[0].ChildNodes)
                        {
                            if (node.Name == "sf:Id")
                            {
                                leadId = node.InnerText;
                            }
                            else if (node.Name == "sf:OwnerId")
                            {
                                accountId = node.InnerText;
                            }
                        }
                    }
                }
                else
                {
                    if (leadId == string.Empty && accountId == string.Empty)
                    {
                        throw new ArgumentException(xmlPayload);
                    }
                }
            }
            catch (ArgumentException e)
            {
                _alertReporter.ImproperSalesforceNotificationReceived(
                    String.Format("Cannot extract  Lead ID and Account ID from notification:, XML Payload\r\n{0}", e.Message));
                throw new ArgumentException();
            }
        }

        private IEnumerable<CrateDTO> CreatePayLoadData(string leadId, string accountId)
        {
            IList<CrateDTO> curEventPayloadData = new List<CrateDTO>();

            var crateFields = new List<FieldDTO>()
                {
                    new FieldDTO() {Key = "LeadID", Value = leadId},
                    new FieldDTO() {Key = "AccountID", Value = accountId},                                   
                };
            curEventPayloadData.Add(_crate.Create("Payload Data", JsonConvert.SerializeObject(crateFields)));

            return curEventPayloadData;
        }
    }
}