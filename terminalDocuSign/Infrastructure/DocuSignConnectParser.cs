using Data.Interfaces.Manifests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using terminalDocuSign.Infrastructure.DocuSignParserModels;

namespace terminalDocuSign.Infrastructure
{
    public static class DocuSignConnectParser
    {
        public static DocuSignEnvelopeInformation GetEnvelopeInformation(string xmlPayload)
        {
            if (string.IsNullOrEmpty(xmlPayload))
                throw new ArgumentNullException("xmlPayload");

            DocuSignEnvelopeInformation docusignEnvelopeInfo;
            var serializer = new XmlSerializer(typeof(DocuSignEnvelopeInformation), "http://www.docusign.net/API/3.0");
            using (var reader = new StringReader(xmlPayload))
            {
                docusignEnvelopeInfo = (DocuSignEnvelopeInformation)serializer.Deserialize(reader);
            }


            if (string.IsNullOrEmpty(docusignEnvelopeInfo.EnvelopeStatus.EnvelopeID))
                throw new ArgumentException("EnvelopeId is not found in XML payload.");

            return docusignEnvelopeInfo;
        }

        public static DocuSignEnvelopeCM_v2 ParseXMLintoCM(DocuSignEnvelopeInformation curDocuSignEnvelopeInfo)
        {
            var result = new DocuSignEnvelopeCM_v2();
            result.EnvelopeId = curDocuSignEnvelopeInfo.EnvelopeStatus.EnvelopeID;
            result.Status = Enum.GetName(typeof(EnvelopeStatusCode), curDocuSignEnvelopeInfo.EnvelopeStatus.Status);
            result.StatusChangedDateTime = DateTime.UtcNow;

            //Recipients
            foreach (var recipient in curDocuSignEnvelopeInfo.EnvelopeStatus.RecipientStatuses)
            {
                var docusignRecipient = new DocuSignRecipientStatus()
                {
                    Type = Enum.GetName(typeof(RecipientTypeCode), recipient.Type),
                    Email = recipient.Email,
                    Name = recipient.UserName,
                    RecipientId = recipient.RecipientId,
                    RoutingOrderId = recipient.RoutingOrder.ToString(),
                    Status = Enum.GetName(typeof(RecipientStatusCode), recipient.Status)
                };

                //Tabs
                if (recipient.TabStatuses != null)
                {
                    foreach (var dstab in recipient.TabStatuses.Where(a => a.CustomTabType != CustomTabType.Radio && a.CustomTabType != CustomTabType.List))
                    {
                        DocuSignTab tab = ParseTab(dstab);
                        docusignRecipient.Tabs.Add(tab);
                    }

                    ParseTabsWithItems(recipient, docusignRecipient);
                }

                result.Recipients.Add(docusignRecipient);
            }

            foreach (var dstemplate in curDocuSignEnvelopeInfo.EnvelopeStatus.DocumentStatuses)
            {
                var template = new DocuSignTemplate();
                template.DocumentId = dstemplate.ID;
                //sadly Connect gives us only a template name
                template.TemplateId = "";
                template.Name = dstemplate.TemplateName;
                result.Templates.Add(template);
            }

            // Connect doesn't provide CourentRoutingOrder. let's assume that it's a highest routingOrderId from recipients who completed/signed
            var completedRecipients = curDocuSignEnvelopeInfo.EnvelopeStatus.RecipientStatuses.Where(a => a.Status == RecipientStatusCode.Completed || a.Status == RecipientStatusCode.Signed);
            if (completedRecipients.Count() > 0)
            {
                result.CurrentRoutingOrderId = completedRecipients.OrderByDescending(a => a.RoutingOrder).First().RoutingOrder.ToString();
            }
            else
                result.CurrentRoutingOrderId = curDocuSignEnvelopeInfo.EnvelopeStatus.RecipientStatuses.First().RoutingOrder.ToString();

            return result;
        }

        // if you change something here - make sure you also changing the way polling populates the manifest to keep constistency
        private static void ParseTabsWithItems(RecipientStatus recipient, DocuSignRecipientStatus docusignRecipient)
        {
            //handle radio group tabs
            foreach (var radioGroup in recipient.TabStatuses.Where(a => a.CustomTabType == CustomTabType.Radio).GroupBy(a => a.TabLabel))
            {
                var first_tab_ingroup = radioGroup.First();
                var radioGroupTab = ParseTab(first_tab_ingroup);

                if (radioGroup.Count() > 0)
                {
                    foreach (var tab_ingroup in radioGroup)
                    {
                        var childTab = ParseTab(tab_ingroup);
                        radioGroupTab.Items.Add(childTab);
                        childTab.Selected = (childTab.Value != null).ToString();
                    }
                }
                else
                // <TabValue>X</TabValue> <- this is how it arrives with connect          
                {
                    radioGroupTab.Selected = (radioGroupTab.Value != null).ToString();
                }
                docusignRecipient.Tabs.Add(radioGroupTab);
            }

            //handle list tabs
            foreach (var dslisttab in recipient.TabStatuses.Where(a => a.CustomTabType == CustomTabType.List))
            {
                DocuSignTab listtab = ParseTab(dslisttab);

                string[] values = dslisttab.TabName.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries); //yes, TabName
                foreach (var ddlbvalue in values)
                {
                    var childItem = new DocuSignTab();
                    childItem.TabLabel = ddlbvalue;
                    childItem.Value = ddlbvalue;
                    childItem.Selected = false.ToString();
                    listtab.Items.Add(childItem);
                }

                //choose selected
                var selectedItem = listtab.Items.Where(a => a.Value == dslisttab.ListSelectedValue).FirstOrDefault();
                if (selectedItem != null)
                    selectedItem.Selected = true.ToString();

                docusignRecipient.Tabs.Add(listtab);
            }

            //here is how the same tabs in the same state are represented throught connect payload and through api call
            // I wish I could kill a guy at DocuSign, who is responsible for that 

            // < TabStatus >                                           "radioGroupTabs": [
            //     <TabType>Custom</TabType>                            {
            //     <Status>Signed</Status>                                "documentId": "1",
            //     <XPosition>250</XPosition>                             "recipientId": "1",
            //     <YPosition>633</YPosition>                             "groupName": "Radio Button 6",
            //     <TabLabel>Radio Button 6</TabLabel>                    "radios": [
            //     <TabName>Radio</TabName>                                 {
            //     <TabValue>X</TabValue>                                     "pageNumber": "1",
            //     <DocumentID>1</DocumentID>                                 "xPosition": "120",
            //     <PageNumber>1</PageNumber>                                 "yPosition": "304",
            //     <ValidationPattern />                                      "value": "Radio",
            //     <CustomTabType>Radio</CustomTabType>                       "selected": "true",
            // </TabStatus>                                                   "tabId": "0a199bbb-2b50-4afd-b559-df110a55c9cd",
            // <TabStatus>                                                    "required": "True",
            //     <TabType>Custom</TabType>                                  "locked": "False"
            //     <Status>Active</Status>                                  },
            //     <XPosition>175</XPosition>                               {
            //     <YPosition>631</YPosition>                                 "pageNumber": "1",
            //     <TabLabel>Radio Button 6</TabLabel>                        "xPosition": "84",
            //     <TabName>Radio</TabName>                                   "yPosition": "303",
            //     <TabValue />                                               "value": "Radio",
            //     <DocumentID>1</DocumentID>                                 "selected": "false",
            //     <PageNumber>1</PageNumber>                                 "tabId": "ac672ae2-192e-4e16-af19-96f2327374a8",
            //     <ValidationPattern />                                      "required": "True",
            //     <CustomTabType>Radio</CustomTabType>                       "locked": "False"
            // </TabStatus>                                                 }
            //<TabStatus>                                                 ],           
            //    <TabType>Custom</TabType>                               "shared": "False",                             
            //    <Status>Signed</Status>                                 "requireInitialOnSharedChange": "false",                 
            //    <XPosition>150</XPosition>                              "requireAll": "false"                              
            //    <YPosition>227</YPosition>                            }                              
            //    <TabLabel>Drop Down 4</TabLabel>                    ],
            //    <TabName>1;2;3</TabName>                            "listTabs": [
            //    <TabValue>1</TabValue>                                {
            //    <DocumentID>1</DocumentID>                              "listItems": [
            //    <PageNumber>1</PageNumber>                                {
            //    <OriginalValue>1</OriginalValue>                            "text": "1",
            //    <ValidationPattern />                                       "value": "1",
            //    <ListSelectedValue>1</ListSelectedValue>                    "selected": "true"
            //    <CustomTabType>List</CustomTabType>                       },
            //</TabStatus>                                                  {
            //                                                                "text": "2",
            //                                                                "value": "2",
            //                                                                "selected": "false"
            //                                                              },
            //                                                              {
            //                                                                "text": "3",
            //                                                                "value": "3",
            //                                                                "selected": "false"
            //                                                              }
            //                                                            ],
            //                                                            "value": "1",
            //                                                            "width": 24,
            //                                                            "shared": "false",
            //                                                            "requireInitialOnSharedChange": "false",
            //                                                            "required": "true",
            //                                                            "locked": "false",
            //                                                            "requireAll": "false",
            //                                                            "tabLabel": "Drop Down 4",
            //                                                            "documentId": "1",
            //                                                            "recipientId": "1",
            //                                                            "pageNumber": "1",
            //                                                            "xPosition": "72",
            //                                                            "yPosition": "109",
            //                                                            "tabId": "81a45590-e83c-49e4-a059-a2834227a98c",
            //                                                            "templateLocked": "false",
            //                                                            "templateRequired": "false"
            //                                                          }
            //                                                        ]

        }

        private static DocuSignTab ParseTab(TabStatus dstab)
        {
            var tab = new DocuSignTab();
            tab.DocumentId = dstab.DocumentID;
            tab.Name = dstab.TabName;
            tab.TabLabel = dstab.TabLabel;
            string tabType = Enum.GetName(typeof(TabTypeCode), dstab.TabType);
            if (tabType == "Custom")
                tabType = Enum.GetName(typeof(CustomTabType), dstab.CustomTabType);
            tab.TabType = tabType;
            tab.Value = dstab.TabValue;
            return tab;
        }

        private static string GetEventNames(DocuSignEnvelopeInformation curDocuSignEnvelopeInfo)
        {
            List<string> result = new List<string>();

            //Envelope events
            if (curDocuSignEnvelopeInfo.EnvelopeStatus != null)
                result.Add("Envelope" + curDocuSignEnvelopeInfo.EnvelopeStatus.Status);

            //Recipinent events
            if (curDocuSignEnvelopeInfo.EnvelopeStatus != null &&
                curDocuSignEnvelopeInfo.EnvelopeStatus.RecipientStatuses != null)
            {
                var recipientEvents = curDocuSignEnvelopeInfo.EnvelopeStatus.RecipientStatuses.Select(s => "Recipient" + Enum.GetName(typeof(RecipientStatusCode), s.Status)).Distinct();
                result.AddRange(recipientEvents);
            }
            return string.Join(",", result);
        }
    }
}