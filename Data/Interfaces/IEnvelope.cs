using System.Collections.Generic;

using DocuSign.Integrations.Client;

using Data.Interfaces.DataTransferObjects;
using Data.Wrappers;

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
        List<EnvelopeDataDTO> GetEnvelopeData(DocuSignEnvelope envelope);

        /// <summary>
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// </summary>
        /// <param name="envelopeId">DocuSign.Integrations.Client.Envelope envelope id.</param>
        /// <returns>
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        List<EnvelopeDataDTO> GetEnvelopeData(string envelopeId);

        /// <summary>
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// </summary>
        /// <param name="envelope">DocuSign.Integrations.Client.Envelope envelope.</param>
        /// <returns>
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        List<EnvelopeDataDTO> GetEnvelopeData(Envelope envelope);

        /// <summary>
        /// Creates payload in JSON format based on field mappings created by user 
        /// and field values retrieved from a DocuSign envelope.
        /// </summary>
        /// <param name="curFieldMappingsJSON">Field mappings created by user for an action.</param>
        /// <param name="envelopeId">Envelope id which is being processed.</param>
        /// <param name="curEnvelopeData">A collection of form fields extracted from the DocuSign envelope.</param>
        PayloadMappingsDTO ExtractPayload(string curFieldMappingsJSON, string envelopeId, IList<EnvelopeDataDTO> curEnvelopeData);
    }
}