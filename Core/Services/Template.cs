using System;
using System.Collections.Generic;
using System.Linq;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;

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
            return _envelope.GetEnvelopeDataByTemplate(templateId).Select(r=>r.Name);

        }

        public List<string> GetMappableSourceFields(Envelope envelope)
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

