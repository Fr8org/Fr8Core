using System.Collections.Generic;

using Data.Interfaces;

using Utilities;

namespace Core.Services
{
    public class Envelope : DocuSign.Integrations.Client.Envelope, IEnvelope
    {
        private string _baseUrl;
        private readonly ITab _tab;
        private readonly ISigner _signer;

        public Envelope()
        {
            //TODO change baseUrl later. Remove it to constructor parameter etc.
            _baseUrl = string.Empty; 

            //TODO move ioc container.
            _tab = new Tab();
            _signer = new Signer();
        }

        /// <summary>
        /// Get Envelope Data from a docusign envelope. 
        /// Each EnvelopeData row is essentially a specific DocuSign "Tab".
        /// </summary>
        /// <param name="envelope">DocuSign.Integrations.Client.Envelope envelope domain.</param>
        /// <returns>
        /// List of Envelope Data.
        /// It returns empty list of envelope data if tab and signers not found.
        /// </returns>
        public List<EnvelopeData> GetEnvelopeData(DocuSign.Integrations.Client.Envelope envelope)
        {
            Signer[] curSignersSet = _signer.GetSignersFromRecipients(envelope);
            if (curSignersSet != null)
            {
                foreach (Signer curSigner in curSignersSet)
                {
                    return _tab.ExtractEnvelopeData(envelope, curSigner);
                }
            }

            return new List<EnvelopeData>();
        }

    }
}
