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
        /// Creates payload as a collection of fields based on field mappings created by user 
        /// and field values retrieved from a DocuSign envelope.
        /// </summary>
        /// <param name="curFields">Field mappings created by user for an action.</param>
        /// <param name="curEnvelopeId">Envelope id which is being processed.</param>
        /// <param name="curEnvelopeData">A collection of form fields extracted from the DocuSign envelope.</param>
        public IList<FieldDTO> ExtractPayload(List<FieldDTO> curFields, string curEnvelopeId,
            IList<EnvelopeDataDTO> curEnvelopeData)
        {
            var payload = new List<FieldDTO>();

            if (curFields != null)
            {
                curFields.ForEach(f =>
                {
                    var newValue = curEnvelopeData.Where(e => e.Name == f.Key).Select(e => e.Value).SingleOrDefault();
                    if (newValue == null)
                    {
                        EventManager.DocuSignFieldMissing(curEnvelopeId, f.Key);
                    }
                    else
                    {
                        payload.Add(new FieldDTO() { Key = f.Key, Value = newValue });
                    }
                });
            }
            return payload;
        }

        public IEnumerable<EnvelopeDataDTO> GetEnvelopeDataByTemplate(string templateId)
        {
            ////////Cache////////
            List<EnvelopeDataDTO> envelopeData = null;
            if (TemplatesStorage.TemplateEnvelopeData.TryGetValue(templateId, out envelopeData))
                return envelopeData;
            else envelopeData = new List<EnvelopeDataDTO>();
            ////////////////////

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
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "numberTabs", "value");
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "formulaTabs", "value");
                    if (tabs["checkboxTabs"] == null)
                    {
                        continue;
                    }
                    envelopeData = AddEnvelopeData(envelopeData, tabs, "checkboxTabs", "selected");
                }
            }

            //cache
            TemplatesStorage.TemplateEnvelopeData.TryAdd(templateId, envelopeData);
            
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

        private List<EnvelopeDataDTO> AddEnvelopeData(List<EnvelopeDataDTO> envelopes, JToken tabs,  string tabName, string tabField)
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

    }
}