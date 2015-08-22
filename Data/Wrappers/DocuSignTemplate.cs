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
        IEnumerable<string> GetMappableSourceFields(int templateId);
    }

    public class DocuSignTemplate : DocuSign.Integrations.Client.Template, IDocuSignTemplate
    {
        private DocuSignEnvelope _docusignEnvelope;

        public DocuSignTemplate()
        {
            _docusignEnvelope = new DocuSignEnvelope();
        }

        public TemplateInfo Create(TemplateInfo submissionData)
        {
            //replace with a real implementation that calls POST /accounts/{accountId}/templates
            //for now, just adding arbitrary id to help with testing
            submissionData.Id = "234";
            return submissionData; 
        }

        public IEnumerable<TemplateInfo> GetByAccount(DockyardAccountDO curDockyardAccount)
        {
            Template curTemplate = new DocuSignTemplate();
            //curTemplate.Login = 
            return new List<TemplateInfo>();
        }


        //TODO: merge these
        public IEnumerable<string> GetMappableSourceFields(int templateId)
        {
            return _docusignEnvelope.GetEnvelopeData(templateId.ToString()).Select(r => r.Name);

        }
        public List<string> GetMappableSourceFields(DocuSignEnvelope envelope)
        {
            List<EnvelopeDataDTO> curLstEnvelopeData = _docusignEnvelope.GetEnvelopeData(envelope);
            List<int> curLstDistinctDocIds = curLstEnvelopeData.Select(x => x.DocumentId).Distinct().ToList();
            if (curLstDistinctDocIds.Count == 1)
            {
                return curLstEnvelopeData.Select(x => x.Name).ToList();
            }
            else if (curLstDistinctDocIds.Count > 1)
            {
                List<string> curLstMappableSourceFields = new List<string>();
                foreach (EnvelopeDataDTO curEnvelopeData in curLstEnvelopeData)
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

    }

}
