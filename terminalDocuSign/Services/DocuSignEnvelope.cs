using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Data.Control;
using Newtonsoft.Json.Linq;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;
using Utilities.Serializers.Json;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Interfaces;
using Signer = terminalDocuSign.Infrastructure.Signer;
using Tab = terminalDocuSign.Infrastructure.Tab;
using Utilities.Configuration.Azure;

namespace terminalDocuSign.Services
{
    public class DocuSignEnvelope : DocuSign.Integrations.Client.Envelope, IDocuSignEnvelope
    {
        private string _baseUrl;
        private readonly ITab _tab;
        private readonly ISigner _signer;
        //Can't use DockYardAccount here - circular dependency
        private readonly DocuSignPackager _docuSignPackager;

        private readonly string _email;
        private readonly string _apiPassword;

        // TODO: remove default auth in future.
        public DocuSignEnvelope()
        {
            //TODO change baseUrl later. Remove it to constructor parameter etc.
            _baseUrl = string.Empty;

            //TODO move ioc container.
            _tab = new Tab();
            _signer = new Signer();

            _docuSignPackager = new DocuSignPackager
            {
                CurrentEmail = CloudConfigurationManager.GetSetting("DocuSignLoginEmail"),
                CurrentApiPassword = CloudConfigurationManager.GetSetting("DocuSignLoginPassword")
            };

            _email = null;
            _apiPassword = null;

            Login = _docuSignPackager.Login();
        }

        public DocuSignEnvelope(string email, string apiPassword)
        {
            //TODO change baseUrl later. Remove it to constructor parameter etc.
            _baseUrl = string.Empty;

            //TODO move ioc container.
            _tab = new Tab();
            _signer = new Signer();

            _docuSignPackager = new DocuSignPackager();

            _email = email;
            _apiPassword = apiPassword;

            Login = _docuSignPackager.Login(email, apiPassword);
        }


        /// <summary>
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </summary>
        public IList<EnvelopeDataDTO> GetEnvelopeData(string curEnvelopeId)
        {
            if (string.IsNullOrEmpty(curEnvelopeId))
            {
                throw new ArgumentNullException("envelopeId");
            }
            EnvelopeId = curEnvelopeId;
            GetRecipients(true, true);
            return GetEnvelopeData(this);
        }

        // TODO: This implementation of the interface method is no different than what is already implemented in the other overload. Hence commenting out here and in the interface definition.
        // If not deleted, this will cause grief as DocuSingEnvelope (object in parameter) is defined in both the plugin project and the Data project  and interface expects it to be in Data.Wrappers 
        // namespace, where it will not belong. 
        //public List<EnvelopeDataDTO> GetEnvelopeData(DocuSignEnvelope envelope)
        //{
        //    Signer[] curSignersSet = _signer.GetFromRecipients(envelope);
        //    if (curSignersSet != null)
        //    {
        //        foreach (var curSigner in curSignersSet)
        //        {
        //            return _tab.ExtractEnvelopeData(envelope, curSigner);
        //        }
        //    }

        //    return new List<EnvelopeDataDTO>();
        //}


        /// <summary>
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// </summary>
        /// <param name="envelope">DocuSign.Integrations.Client.Envelope envelope.</param>
        /// <returns>
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        public IList<EnvelopeDataDTO> GetEnvelopeData(DocuSign.Integrations.Client.Envelope envelope)
        {
            Signer[] curSignersSet = _signer.GetFromRecipients(envelope);
            if (curSignersSet != null)
            {
                foreach (var curSigner in curSignersSet)
                {
                    return _tab.ExtractEnvelopeData(envelope, curSigner);
                }
            }
            return new List<EnvelopeDataDTO>();
        }

        /// <summary>
        /// Creates Envelope payload, based on envelope data and default template fields
        /// </summary>
        /// <param name="curTemplateFields"></param>
        /// <param name="curEnvelopeId"></param>
        /// <param name="curEnvelopeData"></param>
        /// <returns></returns>
        public IList<FieldDTO> FormEnvelopePayload(List<FieldDTO> curTemplateFields, string curEnvelopeId,
            IList<EnvelopeDataDTO> curEnvelopeData)
        {
            var payload = new List<FieldDTO>();

            foreach (var envelopeData in curEnvelopeData)
            {
                payload.Add(new FieldDTO() { Key = envelopeData.Name, Value = envelopeData.Value });
            }

            //add missing values from template
            var missing_fields =
             curTemplateFields.Where(a => !payload.Any(b => b.Key == a.Key));

            payload.AddRange(missing_fields.ToList());

            return payload;
        }

        public IEnumerable<EnvelopeDataDTO> GetEnvelopeDataByTemplate(string templateId)
        {
            var envelopeData = new List<EnvelopeDataDTO>();

            var curDocuSignTemplate = new DocuSignTemplate();

            if (string.IsNullOrEmpty(_email) || string.IsNullOrEmpty(_apiPassword))
            {
                curDocuSignTemplate.Login = new DocuSignPackager().Login();
            }
            else
            {
                curDocuSignTemplate.Login = new DocuSignPackager().Login(_email, _apiPassword);
            }

            var templateDetails = curDocuSignTemplate.GetTemplate(templateId);

            var recipients = templateDetails["recipients"];

            if (recipients != null && recipients["signers"] != null)
            {
                foreach (var signer in recipients["signers"])
                {
                    var tabs = signer["tabs"];
                    if (tabs == null)
                    {
                        continue;
                    }
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "textTabs", "value");
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "fullNameTabs", "value");
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "firstNameTabs", "value");
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "lastNameTabs", "value");
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "companyTabs", "value");
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "titleTabs", "value");
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "noteTabs", "value");
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "numberTabs", "value");
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "formulaTabs", "value");
                    if (tabs["checkboxTabs"] == null)
                    {
                        continue;
                    }
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "checkboxTabs", "selected");

                    //create radio button groups
                    if (tabs["radioGroupTabs"] == null)
                    {
                        continue;
                    }
                    envelopeData = AddRadioButtonGroupEnvelopeData(envelopeData, tabs, "radioGroupTabs", "radios");

                    //create dropdown control as envelope data from tabs
                    if (tabs["listTabs"] == null)
                    {
                        continue;
                    }
                    envelopeData = AdDropdownListEnvelopeData(envelopeData, tabs, "listTabs", "listItems");
                }
            }

            return envelopeData;
        }

        private EnvelopeDataDTO CreateEnvelopeData(dynamic tab, string value)
        {
            return new EnvelopeDataDTO
            {
                DocumentId = tab.documentId,
                RecipientId = tab.recipientId,
                Name = tab.tabLabel,
                TabId = tab.tabId,
                Value = value,
                Type = GetFieldType((string)tab.name)
            };
        }

        public string GetFieldType(string name)
        {
            switch (name)
            {
                case "Checkbox":
                    return ControlTypes.CheckBox;
                case "Text":
                    return ControlTypes.TextBox;
                case "Date Signed":
                    return ControlTypes.DatePicker;
                default:
                    return ControlTypes.TextBox;
            }
        }

        public void SendUsingTemplate(string templateId, string recipientAddress)
        {
            var curEnv = new Envelope();
            var templateList = new List<string> { templateId };
            curEnv.AddTemplates(templateList);
            curEnv.Create();
        }

        private List<EnvelopeDataDTO> AddEnvelopeData(List<EnvelopeDataDTO> envelopes, JToken tabs, string tabName, string tabField)
        {
            if (tabs[tabName] != null)
            {
                foreach (var textTab in tabs[tabName])
                {
                    envelopes.Add(CreateEnvelopeData(textTab, textTab[tabField].ToString()));
                }
            }
            return envelopes;
        }

        private List<EnvelopeDataDTO> AddRadioButtonGroupEnvelopeData(List<EnvelopeDataDTO> envelopes, JToken tabs, string tabName, string radiosName)
        {
            if (tabs[tabName] != null)
            {
                foreach (var groupTab in tabs[tabName])
                {
                    dynamic grpTab = groupTab;
                    var groupWrapperEnvelopeData = new GroupWrapperEnvelopeDataDTO
                    {
                        DocumentId = grpTab.documentId,
                        RecipientId = grpTab.recipientId,
                        Name = grpTab.groupName,
                        TabId = grpTab.tabId,
                        Type = ControlTypes.RadioButtonGroup
                    };

                    if (groupTab[radiosName] != null)
                    {
                        foreach (var listItem in groupTab[radiosName])
                        {
                            dynamic lstItem = listItem;
                            groupWrapperEnvelopeData.Items.Add(new GroupItemEnvelopeDataDTO
                            {
                                Value = lstItem.value,
                                Selected = lstItem.selected //todo: convert from string to bool
                            });
                        }
                    }

                    envelopes.Add(groupWrapperEnvelopeData);
                }
            }
            return envelopes;
        }

        private List<EnvelopeDataDTO> AdDropdownListEnvelopeData(List<EnvelopeDataDTO> envelopes, JToken tabs, string tabName, string listItemsName)
        {
            if (tabs[tabName] != null)
            {
                foreach (var groupTab in tabs[tabName])
                {
                    dynamic grpTab = groupTab;
                    var groupWrapperEnvelopeData = new GroupWrapperEnvelopeDataDTO
                    {
                        DocumentId = grpTab.documentId,
                        RecipientId = grpTab.recipientId,
                        Name = grpTab.tabLabel,
                        TabId = grpTab.tabId,
                        Type = ControlTypes.DropDownList,
                        Value = grpTab.Value
                    };

                    
                    if (groupTab[listItemsName] != null)
                    {
                        foreach (var listItem in groupTab[listItemsName])
                        {
                            dynamic lstItem = listItem;
                            groupWrapperEnvelopeData.Items.Add(new GroupItemEnvelopeDataDTO
                            {
                                Text = lstItem.text,
                                Value = lstItem.value,
                                Selected = lstItem.selected //todo: convert from string to bool
                            });
                        }
                    }

                    envelopes.Add(groupWrapperEnvelopeData);
                }
            }
            return envelopes;
        }
    }
}