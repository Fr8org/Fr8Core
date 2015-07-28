using System.Collections.Generic;

using Utilities;

namespace Core.Services
{
    public interface IEnvelope
    {
        List<EnvelopeData> GetEnvelopeData(DocuSign.Integrations.Client.Envelope docusignEnvelope);
    }

    public class Envelope : IEnvelope
    {
        private string _baseUrl;
        
        public Envelope()
        {

        }

        public List<EnvelopeData> GetEnvelopeData(DocuSign.Integrations.Client.Envelope docusignEnvelope)
        {
            //TODO orkan: when Dockyard retrieves this envelope data, it immediately normalizes it into a set of EnvelopeData objects. 
            //Each EnvelopeData row is essentially a specific DocuSign "Tab". By normalizing in this way, 
            //other parts of the system can make neat queries against this in-memory table. 
            //As a security policy, we do not intend to persist these DocumentData rows; we will generate them fresh from the retrieved DocuSign data each time we need them.

            List<EnvelopeData> envelopeDatas = new List<EnvelopeData>();

            //Each EnvelopeData row is essentially a specific DocuSign "Tab".
            //TODO orkan asking: there is no method line docusignEnvelope.GetTabs etc. How can I reiceve tabs and generate them to List of EnvelopeData ?

            return envelopeDatas;
        }
    }

    //TODO orkan is asking: where do I need to move EnvelopeData?
}
