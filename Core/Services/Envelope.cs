using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Data.Interfaces;
using DocuSign.Integrations.Client;
using Newtonsoft.Json.Linq;
using Utilities;

namespace Core.Services
{
    public class Envelope : DocuSign.Integrations.Client.Envelope, IEnvelope
    {
        private string _baseUrl;
        private readonly ITab _tab;
        private readonly ISigner _signer;

        public Envelope()
        {
            //TODO change baseUrl later. Remove it to constructor parameter etc.
            _baseUrl = string.Empty;

            //TODO move ioc container.
            _tab = new Tab();
            _signer = new Signer();
        }

        /// <summary>
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// </summary>
        /// <param name="envelope">DocuSign.Integrations.Client.Envelope envelope domain.</param>
        /// <returns>
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        public List<EnvelopeData> GetEnvelopeData(DocuSign.Integrations.Client.Envelope envelope)
        {
            Signer[] curSignersSet = _signer.GetFromRecipients(envelope);
            if (curSignersSet != null)
            {
                foreach (Signer curSigner in curSignersSet)
                {
                    return _tab.ExtractEnvelopeData(envelope, curSigner);
                }
            }

            return new List<EnvelopeData>();
        }

        public IEnumerable<EnvelopeData> GetEnvelopeData(string templateId)
        {

            var username = ConfigurationManager.AppSettings["username"];
            var password = ConfigurationManager.AppSettings["password"];
            var integratorKey = ConfigurationManager.AppSettings["IntegratorKey"];
            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];

            if (username == null
                || password == null
                || integratorKey == null
                || baseUrl == null
              )
                throw new ApplicationException(" Web/App Config is missing Docusign values of "
                                                + (username == null ? "username, " : "")
                                                + (password == null ? "password, " : "")
                                                + (integratorKey == null ? "IntegratorKey, " : "")
                                                + (baseUrl == null ? "environment, " : ""));


            RestSettings.Instance.IntegratorKey = integratorKey;


            var template = new DocuSign.Integrations.Client.Template
            {
                Login = new DocuSign.Integrations.Client.Account
                {
                    Email = username,
                    ApiPassword = password,
                    BaseUrl = baseUrl
                }
            };


            var templateDetails = template.GetTemplate(templateId);
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

        private EnvelopeData CreateEnvelopeData(dynamic tab, string value)
        {
            return new EnvelopeData()
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
