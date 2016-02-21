using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;
using terminalDocuSign.Interfaces;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using System.Globalization;
using System.Runtime.Caching;
using Utilities.Configuration.Azure;

namespace terminalDocuSign.Services
{
    public class DocuSignTemplate : Template, IDocuSignTemplate
    {
        private static readonly MemoryCache UserTemplatesCache = new MemoryCache("DocusignTemplateCache");
        private static readonly Dictionary<string, object> EmailLocks = new Dictionary<string, object>();

        // Commented by blazingmind (Vlad). 
        // because _docusignEnvelope is not initialized
        //  private DocuSignEnvelope _docusignEnvelope;
        private readonly DocuSignPackager _docusignPackager;
        private readonly TimeSpan _templateCacheTimeout;

        public DocuSignTemplate()
        {
            _docusignPackager = new DocuSignPackager();
            var timeoutStr = CloudConfigurationManager.GetSetting("DocusignTemplateCacheExpirationTimeout");
            int timeout;

            if (timeoutStr != null && int.TryParse(timeoutStr, NumberStyles.Any, CultureInfo.InvariantCulture, out timeout))
            {
                _templateCacheTimeout = TimeSpan.FromSeconds(timeout);
            }
            else
            {
                _templateCacheTimeout = TimeSpan.FromSeconds(30);
            }
        }

        public TemplateInfo Create(TemplateInfo submissionData)
        {
            //replace with a real implementation that calls POST /accounts/{accountId}/templates
            //for now, just adding arbitrary id to help with testing
            submissionData.Id = "234";
            return submissionData;
        }


        public IEnumerable<TemplateInfo> GetTemplateNames(string email, string apiPassword)
        {
            List<TemplateInfo> templates;
            object mailLock;

            // we'll use personal lock for each email not to block all users while loading template list.
            lock (EmailLocks)
            {
                if (!EmailLocks.TryGetValue(email, out mailLock))
                {
                    mailLock = new object();
                    EmailLocks[email] = mailLock;
                }
            }

            lock (mailLock)
            {
                templates = (List<TemplateInfo>)UserTemplatesCache.Get(email);

                if (templates == null)
                {
                    Login = _docusignPackager.Login(email, apiPassword);

                    templates = GetTemplates();

                    UserTemplatesCache.Add(email, templates, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.Add(_templateCacheTimeout)
                    });
                }
            }

            return templates;
        }

        public DocuSignTemplateDTO GetTemplateById(string email, string apiPassword, string templateId)
        {
            Login = _docusignPackager.Login(email, apiPassword);
            return GetTemplateByIdInternally(templateId);
        }

        private DocuSignTemplateDTO GetTemplateByIdInternally(string templateId)
        {
            if (templateId == null)
                throw new ArgumentNullException("templateId");
            if (templateId == string.Empty)
                throw new ArgumentException("templateId is empty", "templateId");
            // Get template
            var jObjTemplate = GetTemplate(templateId);
            // Checking is it ok?
            DocuSignUtils.ThrowInvalidOperationExceptionIfError(jObjTemplate);
            return Mapper.Map<DocuSignTemplateDTO>(jObjTemplate);
        }

        public DocuSignTemplateDTO GetTemplateById(string templateId)
        {
            return GetTemplateByIdInternally(templateId);
        }

        // Commented by blazingmind (Vlad). Current implementation of these methods will fail 
        // because _docusignEnvelope is not initialized
        // If you need them you have to fix DocuSignTemplate service logic first
        //TODO: merge these
        /*public IEnumerable<string> GetMappableSourceFields(string templateId)
        {
            return _docusignEnvelope.GetEnvelopeDataByTemplate(templateId).Select(r => r.Name);
        }

        public List<string> GetMappableSourceFields(DocuSignEnvelope envelope)
        {
            IList<EnvelopeDataDTO> curEnvelopeDataList = _docusignEnvelope.GetEnvelopeData(envelope);
            List<int> curDistinctDocIds = curEnvelopeDataList.Select(x => x.DocumentId).Distinct().ToList();
            if (curDistinctDocIds.Count == 1)
            {
                return curEnvelopeDataList.Select(x => x.Name).ToList();
            }

            if (curDistinctDocIds.Count <= 1)
                return null;

            //add the document name as a suffix if there's more than one document involved
            var curLstMappableSourceFields = new List<string>();
            foreach (var curEnvelopeData in curEnvelopeDataList)
            {
                EnvelopeDocuments curEnvelopDocuments = envelope.GetEnvelopeDocumentInfo(curEnvelopeData.EnvelopeId);
                List<EnvelopeDocument> curLstenvelopDocuments = curEnvelopDocuments
                     .envelopeDocuments.ToList()
                     .Where(x => Convert.ToInt32(x.documentId) == curEnvelopeData.DocumentId).ToList();
                curLstMappableSourceFields.Add(curEnvelopeData.Name + " from " + curLstenvelopDocuments[0].name);
            }
            return curLstMappableSourceFields;
        }*/

        public List<string> GetUserFields(DocuSignTemplateDTO curDocuSignTemplateDTO)
        {
            if (curDocuSignTemplateDTO == null)
                throw new ArgumentNullException("curDocuSignTemplateDTO");

            Recipients recipient = curDocuSignTemplateDTO.EnvelopeData.recipients;
            // TODO Do we need to get textTabs for other types of recipients?
            var allSigners = recipient.signers.Concat(recipient.agents)
                .Concat(recipient.carbonCopies)
                .Concat(recipient.certifiedDeliveries)
                .Concat(recipient.editors)
                .Concat(recipient.inPersonSigners)
                .Concat(recipient.intermediaries).ToArray();

            // The only thing about it that we care about is the "value" property.
            List<string> values = new List<string>();
            foreach (var singer in allSigners)
            {
                if (singer.tabs != null && singer.tabs.textTabs != null)
                    values.AddRange(singer.tabs.textTabs.Select(x => x.value));
            }
            return values;
        }

    }
}