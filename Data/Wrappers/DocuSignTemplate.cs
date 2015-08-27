using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;

namespace Data.Wrappers
{
    public interface IDocuSignTemplate
    {
        List<string> GetMappableSourceFields(DocuSignEnvelope envelope);
        IEnumerable<string> GetMappableSourceFields(string templateId);
        IEnumerable<DocuSignTemplateDTO> GetTemplates(DockyardAccountDO curDockyardAccount);

        /// <summary>
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// </summary>
        /// <param name="templateId">templateId</param>
        /// <returns>
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        IEnumerable<EnvelopeDataDTO> GetEnvelopeDataByTemplate(string templateId);

    }

    public class DocuSignTemplate : Template, IDocuSignTemplate
    {
        private readonly DocuSignEnvelope _docusignEnvelope;
        private DocuSignTemplate _docusignTemplate;
        private DocuSignAccount _account;

        public DocuSignTemplate()
        {
            var packager = new DocuSignPackager();
            //_account = packager.Login();
            //Login = _account;
            Login = packager.LoginAsDockyardService();
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
            return GetEnvelopeDataByTemplate(templateId).Select(r => r.Name);

        }

        public List<string> GetMappableSourceFields(DocuSignEnvelope envelope)
        {
            List<EnvelopeDataDTO> curEnvelopeDataList = _docusignEnvelope.GetEnvelopeData(envelope);
            List<int> curDistinctDocIds = curEnvelopeDataList.Select(x => x.DocumentId).Distinct().ToList();
            if (curDistinctDocIds.Count == 1)
            {
                return curEnvelopeDataList.Select(x => x.Name).ToList();
            }
            else if (curDistinctDocIds.Count > 1)
            {
                //add the document name as a suffix if there's more than one document involved
                List<string> curLstMappableSourceFields = new List<string>();
                foreach (EnvelopeDataDTO curEnvelopeData in curEnvelopeDataList)
                {
                    EnvelopeDocuments curEnvelopDocuments = envelope.GetEnvelopeDocumentInfo(curEnvelopeData.EnvelopeId);
                    List<EnvelopeDocument> curLstenvelopDocuments = curEnvelopDocuments
                        .envelopeDocuments.ToList()
                        .Where(x => Convert.ToInt32(x.documentId) == curEnvelopeData.DocumentId).ToList();
                    curLstMappableSourceFields.Add(curEnvelopeData.Name + " from " + curLstenvelopDocuments[0].name);
                }
                return curLstMappableSourceFields;
            }
            else
            {
                return null;
            }
        }
    }
}