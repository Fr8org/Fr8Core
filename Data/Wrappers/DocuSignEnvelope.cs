using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;

namespace Data.Wrappers
{
    public class DocuSignEnvelope : Envelope, IEnvelope
    {
        private string _baseUrl;
        private readonly ITab _tab;
        private readonly ISigner _signer;
        //Can't use DockYardAccount here - circular dependency
        private readonly DocuSignPackager _docuSignPackager;

        public DocuSignEnvelope()
        {
            //TODO change baseUrl later. Remove it to constructor parameter etc.
            _baseUrl = string.Empty;

            //TODO move ioc container.
            _tab = new Tab();
            _signer = new Signer();

            var packager = new DocuSignPackager();
            packager.Email = ConfigurationManager.AppSettings["DocuSignLoginEmail"];
            packager.ApiPassword = ConfigurationManager.AppSettings["DocuSignLoginPassword"];
            Login = packager.LoginAsDockyardService();

            _docuSignPackager = new DocuSignPackager();
        }


        /// <summary>
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        /// 
        /// 
        public List<EnvelopeDataDTO> GetEnvelopeData(string curEnvelopeId)
        {
            if (string.IsNullOrEmpty(curEnvelopeId))
            {
                throw new ArgumentNullException("envelopeId");
            }
            EnvelopeId = curEnvelopeId;
            GetRecipients(true, true);
            return GetEnvelopeData(this);
        }

        public List<EnvelopeDataDTO> GetEnvelopeData(DocuSignEnvelope envelope)
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
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// </summary>
        /// <param name="envelope">DocuSign.Integrations.Client.Envelope envelope.</param>
        /// <returns>
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        public List<EnvelopeDataDTO> GetEnvelopeData(Envelope envelope)
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
        /// Creates payload in JSON format based on field mappings created by user 
        /// and field values retrieved from a DocuSign envelope.
        /// </summary>
        /// <param name="curFieldMappingsJSON">Field mappings created by user for an action.</param>
        /// <param name="curEnvelopeId">Envelope id which is being processed.</param>
        /// <param name="curEnvelopeData">A collection of form fields extracted from the DocuSign envelope.</param>
        public PayloadMappingsDTO ExtractPayload(string curFieldMappingsJSON, string curEnvelopeId,
            IList<EnvelopeDataDTO> curEnvelopeData)
        {
            var mappings = new FieldMappingSettingsDTO();
            mappings.Deserialize(curFieldMappingsJSON);
            var payload = new PayloadMappingsDTO();

            mappings.ForEach(m =>
            {
                var newValue = curEnvelopeData.Where(e => e.Name == m.Name).Select(e => e.Value).SingleOrDefault();
                if (newValue == null)
                {
                    EventManager.DocuSignFieldMissing(curEnvelopeId, m.Name);
                }
                else
                {
                    payload.Add(new FieldMappingDTO() {Name = m.Name, Value = newValue});
                }
            });
            return payload;
        }

        public IEnumerable<EnvelopeDataDTO> GetEnvelopeDataByTemplate(string templateId)
        {
            var curDocuSignTemplate = new DocuSignTemplate
            {
                Login = _docuSignPackager.LoginAsDockyardService()
            };

            var templateDetails = curDocuSignTemplate.GetTemplate(templateId);
            foreach (var signer in templateDetails["recipients"]["signers"])
            {
                if (signer["tabs"]["textTabs"] != null)
                    foreach (var textTab in signer["tabs"]["textTabs"])
                    {
                        yield return CreateEnvelopeData(textTab, textTab["value"].ToString());
                    }
                if (signer["tabs"]["checkboxTabs"] == null) continue;
                foreach (var chekBoxTabs in signer["tabs"]["checkboxTabs"])
                {
                    yield return CreateEnvelopeData(chekBoxTabs, chekBoxTabs["selected"].ToString());
                }
            }
        }

        private EnvelopeDataDTO CreateEnvelopeData(dynamic tab, string value)
        {
            return new EnvelopeDataDTO
            {
                DocumentId = tab.documentId,
                RecipientId = tab.recipientId,
                Name = tab.name,
                TabId = tab.tabId,
                Value = value
            };
        }
    }
}