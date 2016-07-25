using DocuSign.eSign.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Fr8.Infrastructure.Data.Manifests;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure.DocuSignParserModels;
using terminalDocuSign.Services.NewApi;

namespace terminalDocuSign.Infrastructure
{
    public static class DocuSignEventParser
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
            result.Subject = curDocuSignEnvelopeInfo.EnvelopeStatus.Subject;
            result.ExternalAccountId = curDocuSignEnvelopeInfo.EnvelopeStatus.Email;
            result.SentDate = curDocuSignEnvelopeInfo.EnvelopeStatus.Sent;
            result.CreateDate = curDocuSignEnvelopeInfo.EnvelopeStatus.Created;
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
                        DocuSignTabStatus tab = ParseTab(dstab);
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

        public static DocuSignEnvelopeCM_v2 ParseAPIresponsesIntoCM(out DocuSignEnvelopeCM_v2 envelope, TemplateInformation templates, Recipients recipients)
        {
            envelope = new DocuSignEnvelopeCM_v2();
            envelope.CurrentRoutingOrderId = recipients.CurrentRoutingOrder;
            

            if (templates.Templates != null)
                foreach (var ds_template in templates.Templates)
                {
                    DocuSignTemplate template = new DocuSignTemplate();
                    template.DocumentId = ds_template.DocumentId;
                    template.Name = ds_template.Name;
                    template.TemplateId = ds_template.TemplateId;
                    envelope.Templates.Add(template);
                }

            //Recipients

            if (recipients.Signers != null)
                foreach (var dsrecipient in recipients.Signers)
                {
                    DocuSignRecipientStatus recipient = new DocuSignRecipientStatus();
                    recipient.Email = dsrecipient.Email;
                    recipient.Name = dsrecipient.Name;
                    recipient.RecipientId = dsrecipient.RecipientId;
                    recipient.RoutingOrderId = dsrecipient.RoutingOrder;
                    recipient.Status = dsrecipient.Status;
                    recipient.Type = "Signer";
                    envelope.Recipients.Add(recipient);

                    //Tabs
                    if (dsrecipient.Tabs != null)
                    {
                        var tabsDTO = DocuSignTab.ExtractTabs(JObject.Parse(dsrecipient.Tabs.ToJson()), "");

                        foreach (var tabDTO in tabsDTO)
                        {
                            DocuSignTabStatus tab = new DocuSignTabStatus();
                            tab.DocumentId = tabDTO.DocumentId.ToString();
                            tab.Name = tabDTO.Name;
                            tab.TabType = tabDTO.Type;
                            tab.Value = tabDTO.Value;
                            tab.TabLabel = tabDTO.TabLabel;

                            if (tabDTO is DocuSignMultipleOptionsTabDTO)
                            {
                                var multiTabDTO = (DocuSignMultipleOptionsTabDTO)tabDTO;
                                foreach (var childDTO in multiTabDTO.Items)
                                {
                                    var childTab = new DocuSignTabStatus();
                                    childTab.Selected = childDTO.Selected.ToString();
                                    childTab.Value = childDTO.Value;
                                    childTab.TabLabel = childDTO.Text;
                                    tab.Items.Add(childTab);
                                }
                            }
                            recipient.Tabs.Add(tab);
                        }
                    }
                }
            return envelope;
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
                DocuSignTabStatus listtab = ParseTab(dslisttab);

                string[] values = dslisttab.TabName.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries); //yes, TabName
                foreach (var ddlbvalue in values)
                {
                    var childItem = new DocuSignTabStatus();
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

        }

        private static DocuSignTabStatus ParseTab(TabStatus dstab)
        {
            var tab = new DocuSignTabStatus();
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

        public static string GetEventNames(DocuSignEnvelopeInformation curDocuSignEnvelopeInfo)
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

        public static string GetEventNames(DocuSignEnvelopeCM_v2 eventManifest)
        {
            List<string> result = new List<string>();

            //Envelope events
            if (eventManifest.Status != null)
                result.Add("Envelope" + eventManifest.Status);

            //Recipinent events
            if (eventManifest.Recipients.Count != 0)
            {
                var recipientEvents = eventManifest.Recipients.Select(s => "Recipient" + s.Status).Distinct();
                result.AddRange(recipientEvents);
            }
            return string.Join(",", result);
        }

    }
}