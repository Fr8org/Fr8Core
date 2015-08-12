using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Utilities;
using Newtonsoft.Json.Linq;

namespace Core.Services
{
    public class Template : ITemplate
    {
        private readonly Envelope _envelop;

        public Template()
        {
            _envelop = new Envelope();
        }

        public List<string> GetMappableSourceFields(DocuSign.Integrations.Client.Envelope envelop)
        {
            List<EnvelopeData> items = _envelop.GetEnvelopeData(envelop);
            List<int> distinctDocIds = items.Select(x => x.DocumentId).Distinct().ToList();
            if (distinctDocIds.Count == 1) 
            {
                return items.Select(x => x.Name).ToList();
            }
            else if (distinctDocIds.Count > 1)
            {
                List<string> data = new List<string>();
                foreach (EnvelopeData ed in items)
                {
                    DocuSign.Integrations.Client.EnvelopeDocuments envelopDocuments = envelop.GetEnvelopeDocumentInfo(ed.EnvelopeId);
                    List<DocuSign.Integrations.Client.EnvelopeDocument> lstenvelopDocuments = envelopDocuments.envelopeDocuments.ToList().Where(x => Convert.ToInt32(x.documentId) == ed.DocumentId).ToList();
                    data.Add(ed.Name + " from " + lstenvelopDocuments[0].name);
                }
                return data;
            }
            else 
            {
                return null;
            }
        }
    }
}

