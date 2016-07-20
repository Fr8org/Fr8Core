using System.Collections.Generic;
using Fr8Data.DataTransferObjects;

namespace Fr8Data.Manifests
{
    public class DocuSignEnvelopeCM_v3 : Manifest
    {
        public List<DocuSignEnvelopeDTO> Envelopes { get; set; }

        public DocuSignEnvelopeCM_v3()
              : base(Constants.MT.DocuSignEnvelope_v3)
        {
        }
    }
}
