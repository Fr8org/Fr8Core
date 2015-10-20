using System.Collections.Generic;
using DocuSign.Integrations.Client;
using Data.Interfaces.DataTransferObjects;

namespace terminalDocuSign.Interfaces
{
    public interface IDocuSignEnvelope
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
        //List<EnvelopeDataDTO> GetEnvelopeData(DocuSignEnvelope envelope);

        /// <summary>
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// </summary>
        /// <param name="envelopeId">DocuSign.Integrations.Client.Envelope envelope id.</param>
        /// <returns>
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        IList<EnvelopeDataDTO> GetEnvelopeData(string envelopeId);

        /// <summary>
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// </summary>
        /// <param name="envelope">DocuSign.Integrations.Client.Envelope envelope.</param>
        /// <returns>
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        IList<EnvelopeDataDTO> GetEnvelopeData(Envelope envelope);

        /// <summary>
        /// Creates payload based on field mappings created by user 
        /// and field values retrieved from a DocuSign envelope.
        /// </summary>
        /// <param name="curFields">Field mappings created by user for an action.</param>
        /// <param name="envelopeId">Envelope id which is being processed.</param>
        /// <param name="curEnvelopeData">A collection of form fields extracted from the DocuSign envelope.</param>
        IList<FieldDTO> ExtractPayload(List<FieldDTO> curFields, string envelopeId, IList<EnvelopeDataDTO> curEnvelopeData);

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

        void SendUsingTemplate(string templateId, string recipientAddress);
    }
}