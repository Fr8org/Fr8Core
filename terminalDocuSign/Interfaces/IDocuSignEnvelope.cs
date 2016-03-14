using System.Collections.Generic;
using DocuSign.Integrations.Client;
using Data.Interfaces.DataTransferObjects;
using terminalDocuSign.DataTransferObjects;

namespace terminalDocuSign.Interfaces
{
    //public interface IDocuSignEnvelope
    //{
    //    /// <summary>
    //    /// Get Envelope Data from a docusign envelope. 
    //    /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
    //    /// </summary>
    //    /// <param name="envelope">DocuSign.Integrations.Client.Envelope envelope domain.</param>
    //    /// <returns>
    //    /// List of Envelope Data.
    //    /// It returns empty list of envelope data if tab and signers not found.
    //    /// </returns>
    //    //List<EnvelopeDataDTO> GetEnvelopeData(DocuSignEnvelope envelope);

    //    /// <summary>
    //    /// Get Envelope Data from a docusign envelope. 
    //    /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
    //    /// </summary>
    //    /// <param name="envelopeId">DocuSign.Integrations.Client.Envelope envelope id.</param>
    //    /// <returns>
    //    /// List of Envelope Data.
    //    /// It returns empty list of envelope data if tab and signers not found.
    //    /// </returns>
    //    //IList<DocuSignTabDTO> GetEnvelopeData(string envelopeId);

    //    /// <summary>
    //    /// Get Envelope Data from a docusign envelope. 
    //    /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
    //    /// </summary>
    //    /// <param name="envelope">DocuSign.Integrations.Client.Envelope envelope.</param>
    //    /// <returns>
    //    /// List of Envelope Data.
    //    /// It returns empty list of envelope data if tab and signers not found.
    //    /// </returns>
    //    //IList<DocuSignTabDTO> GetEnvelopeData(Envelope envelope);

    //    /// <summary>
    //    /// Creates Envelope payload, based on default template fields and added custom values
    //    /// </summary>
    //    /// <param name="curTemplateFields"></param>
    //    /// <param name="curEnvelopeId"></param>
    //    /// <param name="curEnvelopeData"></param>
    //    /// <returns></returns>
    //    //IList<FieldDTO> FormEnvelopePayload(List<FieldDTO> curTemplateFields, string curEnvelopeId,
    //    //    IList<DocuSignTabDTO> curEnvelopeData);

    //    /// <summary>
    //    /// Get Envelope Data from a docusign envelope. 
    //    /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
    //    /// </summary>
    //    /// <param name="templateId">templateId</param>
    //    /// <returns>
    //    /// List of Envelope Data.
    //    /// It returns empty list of envelope data if tab and signers not found.
    //    /// </returns>
    //    //IEnumerable<DocuSignTabDTO> GetEnvelopeDataByTemplate(string templateId);

    //    //void SendUsingTemplate(string templateId, string recipientAddress);
    //}
}