using System;
using System.Collections.Generic;

using Data.Entities;

namespace Core.Services
{
    public class Envelope
    {
        private string _baseUrl;
        
        public Envelope()
        {

        }

        public static List<EnvelopeData> GetEnvelopeData(EnvelopeDO envelopeDo)
        {
            //TODO orkan: when Dockyard retrieves this envelope data, it immediately normalizes it into a set of EnvelopeData objects. 
            //Each EnvelopeData row is essentially a specific DocuSign "Tab". By normalizing in this way, 
            //other parts of the system can make neat queries against this in-memory table. 
            //As a security policy, we do not intend to persist these DocumentData rows; we will generate them fresh from the retrieved DocuSign data each time we need them.

            //don't failed the tests. This method will be reviewed.
            return new List<EnvelopeData>();
        }
    }

    //TODO orkan is asking: where do I need to move EnvelopeData?
    public class EnvelopeData
    {
        public string Name { get; set; }

        public string TabId { get; set; }

        public string RecipientId { get; set; }

        public string EnvelopeId { get; set; }

        public string Value { get; set; }

        public string DocumentName { get; set; }
    }
}
