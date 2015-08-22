using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuSign.Integrations.Client;
using Utilities;

namespace Data.Wrappers
{
    public class DocuSignEnvelope : DocuSign.Integrations.Client.Envelope, IEnvelope
    {
        private string _baseUrl;
        private readonly ITab _tab;
        private readonly ISigner _signer;

        public DocuSignEnvelope()
        {
            //TODO change baseUrl later. Remove it to constructor parameter etc.
            _baseUrl = string.Empty;

            //TODO move ioc container.
            _tab = new Tab();
            _signer = new Signer();

            Login = EnsureLogin();
        }

        private DocuSign.Integrations.Client.Account EnsureLogin()
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            string username = appSettings["username"] ?? "Not Found";
            string password = appSettings["password"] ?? "Not Found";
            string integratorKey = appSettings["IntegratorKey"] ?? "Not Found";

            // configure application's integrator key and webservice url
            RestSettings.Instance.IntegratorKey = appSettings["IntegratorKey"];
            RestSettings.Instance.DocuSignAddress = appSettings["environment"];
            RestSettings.Instance.WebServiceUrl = RestSettings.Instance.DocuSignAddress + "/restapi/v2";

            // credentials for sending account
            Account account = new Account();
            account.Email = username;
            account.Password = password;

            // make the Login API call
            bool result = account.Login();

            if (!result)
            {
                throw new InvalidOperationException("Cannot log in to DocuSign. Please check the authentication information on web.config.");
            }
            return account;
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
        public List<EnvelopeDataDTO> GetEnvelopeData(DocuSignEnvelope envelope)
        {
            Signer[] curSignersSet = _signer.GetFromRecipients(envelope);
            if (curSignersSet != null)
            {
                foreach (Signer curSigner in curSignersSet)
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
        /// <param name="curEnvelopeId">DocuSign.Integrations.Client.Envelope envelope id.</param>
        /// <returns>
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        public List<EnvelopeDataDTO> GetEnvelopeData(string curEnvelopeId)
        {
            if (String.IsNullOrEmpty(curEnvelopeId))
            {
                throw new ArgumentNullException("envelopeId");
            }
            EnvelopeId = curEnvelopeId;
            GetRecipients(true, true);
            return GetEnvelopeData(this);
        }

        private List<EnvelopeDataDTO> GetEnvelopeData()
        {
            Signer[] curSignersSet = _signer.GetFromRecipients(this);
            if (curSignersSet != null)
            {
                foreach (Signer curSigner in curSignersSet)
                {
                    return _tab.ExtractEnvelopeData(this, curSigner);
                }
            }

            return new List<EnvelopeDataDTO>();
        }


        public List<EnvelopeDataDTO> GetEnvelopeData(Envelope envelope)
        {
            Signer[] curSignersSet = _signer.GetFromRecipients(envelope);
            if (curSignersSet != null)
            {
                foreach (Signer curSigner in curSignersSet)
                {
                    return _tab.ExtractEnvelopeData(envelope, curSigner);
                }
            }

            return new List<EnvelopeDataDTO>();
        }

        public IEnumerable<EnvelopeDataDTO> GetEnvelopeData(string templateId)
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

        private EnvelopeDataDTO CreateEnvelopeData(dynamic tab, string value)
        {
            return new EnvelopeDataDTO()
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
