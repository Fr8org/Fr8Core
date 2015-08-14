using System.Collections.Generic;

using DocuSign.Integrations.Client;

using Utilities;

namespace Data.Interfaces
{
    public interface IEnvelope
    {
        /// <summary>
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// </summary>
        /// <param name="envelope">DocuSign.Integrations.Client.Envelope envelope domain.</param>
        /// <returns>
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        List<EnvelopeData> GetEnvelopeData(Envelope envelope);

        /// <summary>
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// </summary>
        /// <param name="templateId">templateId</param>
        /// <returns>
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        IEnumerable<EnvelopeData> GetEnvelopeData(string templateId);
    }
}