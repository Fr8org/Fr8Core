using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class DocuSignEnvelopeCM_v3 : Manifest
    {
        public List<DocuSignEnvelopeDTO> Envelopes { get; set; }

        public DocuSignEnvelopeCM_v3()
              : base(MT.DocuSignEnvelope_v3)
        {
        }
    }
}
