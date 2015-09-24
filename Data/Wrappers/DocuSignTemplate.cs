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

namespace Data.Wrappers
{
    public class DocuSignTemplate : Template, IDocuSignTemplate
    {
        private readonly DocuSignEnvelope _docusignEnvelope;

        public DocuSignTemplate()
        {
            var packager = new DocuSignPackager();

            Login = packager.Login();
            _docusignEnvelope = new DocuSignEnvelope();
        }

        public TemplateInfo Create(TemplateInfo submissionData)
        {
            //replace with a real implementation that calls POST /accounts/{accountId}/templates
            //for now, just adding arbitrary id to help with testing
            submissionData.Id = "234";
            return submissionData;
        }

        public IEnumerable<DocuSignTemplateDTO> GetTemplates(DockyardAccountDO curDockyardAccount)
        {
            //TODO: implement getting templates by the specified account.
            return GetTemplates().Select(t => Mapper.Map<DocuSignTemplateDTO>(t));
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
	 }
}