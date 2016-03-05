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
using terminalDocuSign.DataTransferObjects;
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
        public IList<DocuSignTabDTO> GetEnvelopeData(string curEnvelopeId)
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
        public IList<DocuSignTabDTO> GetEnvelopeData(DocuSign.Integrations.Client.Envelope envelope)
        {
            Signer[] curSignersSet = _signer.GetFromRecipients(envelope);
            if (curSignersSet != null)
            {
                foreach (var curSigner in curSignersSet)
                {
                    return _tab.ExtractEnvelopeData(envelope, curSigner);
                }
            }
            return new List<DocuSignTabDTO>();
        }

        /// <summary>
        /// Creates Envelope payload, based on envelope data and default template fields
        /// </summary>
        /// <param name="curTemplateFields"></param>
        /// <param name="curEnvelopeId"></param>
        /// <param name="curEnvelopeData"></param>
        /// <returns></returns>
        public IList<FieldDTO> FormEnvelopePayload(List<FieldDTO> curTemplateFields, string curEnvelopeId,
            IList<DocuSignTabDTO> curEnvelopeData)
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

        public JObject GetTemplateDetails(string templateId)
        {
            var envelopeData = new List<DocuSignTabDTO>();
            var curDocuSignTemplate = new DocuSignTemplate();

            if (string.IsNullOrEmpty(_email) || string.IsNullOrEmpty(_apiPassword))
            {
                curDocuSignTemplate.Login = new DocuSignPackager().Login();
            }
            else
            {
                curDocuSignTemplate.Login = new DocuSignPackager().Login(_email, _apiPassword);
            }

            return curDocuSignTemplate.GetTemplate(templateId);
        }

        public List<FieldDTO> GetTemplateRoles(JObject templateDetails)
        {
            var result = new List<FieldDTO>();
            var recipients = templateDetails["recipients"];
            if (recipients != null && recipients["signers"] != null)
            {
                foreach (var signer in recipients["signers"])
                {
                    string rolename = signer["roleName"] == null 
                        ? (signer["name"] == null 
                            ? signer["email"].ToString() : signer["name"].ToString()) 
                        : signer["roleName"].ToString();
                    result.Add(new FieldDTO(string.Format("{0} role name", rolename)) { Tags = "recipientId:" + signer["recipientId"].ToString() });
                    result.Add(new FieldDTO(string.Format("{0} role email", rolename)) { Tags = "recipientId:" + signer["recipientId"].ToString() });
                }
            }
            return result;
        }

        public IEnumerable<DocuSignTabDTO> GetTemplateEnvelopeData(JObject templateDetails)
        {
            //var envelopeData = new List<DocuSignTabDTO>();
            //envelopeData = ExtractTabs(envelopeData, templateDetails).ToList();

            //return envelopeData;
            return null;
        }

        public IEnumerable<DocuSignTabDTO> GetEnvelopeDataByTemplate(string templateId)
        {
            //var envelopeData = new List<DocuSignTabDTO>();

            //var curDocuSignTemplate = new DocuSignTemplate();

            //if (string.IsNullOrEmpty(_email) || string.IsNullOrEmpty(_apiPassword))
            //{
            //    curDocuSignTemplate.Login = new DocuSignPackager().Login();
            //}
            //else
            //{
            //    curDocuSignTemplate.Login = new DocuSignPackager().Login(_email, _apiPassword);
            //}

            //var templateDetails = curDocuSignTemplate.GetTemplate(templateId);

            //return ExtractTabs(envelopeData, templateDetails);
            return null;
        }

  



        public void SendUsingTemplate(string templateId, string recipientAddress)
        {
            var curEnv = new Envelope();
            var templateList = new List<string> { templateId };
            curEnv.AddTemplates(templateList);
            curEnv.Create();
        }

   
    }
}