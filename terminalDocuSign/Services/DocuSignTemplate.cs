using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using Utilities;
using Data.Interfaces;
using terminalDocuSign.Interfaces;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using System.Configuration;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace terminalDocuSign.Services
{
    //cache for DocuSign templates and template fields
    //invalidation logic is not yet implemented
    public static class TemplatesStorage
    {
        //standard thread-safe dictionaies
        public static ConcurrentDictionary<string, List<TemplateInfo>> UserTemplates = new ConcurrentDictionary<string, List<TemplateInfo>>();

        public static ConcurrentDictionary<string, List<EnvelopeDataDTO>> TemplateEnvelopeData = new ConcurrentDictionary<string, List<EnvelopeDataDTO>>();
    }


    public class DocuSignTemplate : Template, IDocuSignTemplate
    {
        private DocuSignEnvelope _docusignEnvelope;
        private DocuSignPackager _docusignPackager;

        public DocuSignTemplate()
        {
            _docusignPackager = new DocuSignPackager();
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
            List<TemplateInfo> templates = null;
            if (!TemplatesStorage.UserTemplates.TryGetValue(email, out templates))
            {
                //no templates cached for this email
                Login = _docusignPackager.Login(email, apiPassword);
                templates = GetTemplates();
                TemplatesStorage.UserTemplates.TryAdd(email, templates);
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

        //TODO: merge these
        public IEnumerable<string> GetMappableSourceFields(string templateId)
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
        }

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