using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using DocuSign.Integrations.Client;
using Newtonsoft.Json.Linq;
using StructureMap;
using Utilities;

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

    public class DocuSignTemplate : DocuSign.Integrations.Client.Template, IDocuSignTemplate
    {
        private DocuSignEnvelope _docusignEnvelope;
        private DocuSignTemplate _docusignTemplate;
        DocuSignAccount _account;

        public DocuSignTemplate()
        {
            var packager = new DocuSignPackager();
            _account = packager.Login();
            Login = _account;
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
            return GetTemplates().Select(t => AutoMapper.Mapper.Map<DocuSignTemplateDTO>(t));
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
                    DocuSign.Integrations.Client.EnvelopeDocuments curEnvelopDocuments = envelope.GetEnvelopeDocumentInfo(curEnvelopeData.EnvelopeId);
                    List<DocuSign.Integrations.Client.EnvelopeDocument> curLstenvelopDocuments = curEnvelopDocuments
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

        public IEnumerable<EnvelopeDataDTO> GetEnvelopeDataByTemplate(string templateId)
        {
            var templateDetails = GetTemplate(templateId);
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
