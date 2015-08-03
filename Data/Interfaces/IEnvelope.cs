using System.Collections.Generic;

using DocuSign.Integrations.Client;

using Utilities;

namespace Data.Interfaces
{
    public interface IEnvelope
    {
        List<EnvelopeData> GetEnvelopeData(Envelope docusignEnvelope);
    }
}