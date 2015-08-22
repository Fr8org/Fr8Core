using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using DocuSign.Integrations.Client;
using Utilities;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Data.Wrappers;


namespace Core.Services
{
    public class DocuSignxTemplate 
    {
        private readonly IEnvelope _envelope;

        public DocuSignxTemplate()
        {
            //_envelope = ObjectFactory.GetInstance<IEnvelope>();


        }

        public IEnumerable<string> GetMappableSourceFields(string templateId)
        {
            return _envelope.GetEnvelopeData(templateId).Select(r=>r.Name);

        }

        public List<string> GetMappableSourceFields(DocuSign.Integrations.Client.Envelope envelope)
        {
            List<EnvelopeDataDTO> curLstEnvelopeData = _envelope.GetEnvelopeData(envelope);
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

