using System;

using Data.Entities;

using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
	{
		public static EnvelopeDO TestEnvelope1()
		{
		    return new EnvelopeDO
		           {
		               DocusignEnvelopeId = "21",
		               Status = EnvelopeDO.EnvelopeState.Any
		           };
		}

        public static Envelope TestEnvelope2(Account account)
        {
            // create envelope object and assign login info
            return new Envelope
                   {
                       // assign account info from above
                       Login = account,
                       // "sent" to send immediately, "created" to save envelope as draft
                       Status = "created",
                       Created = DateTime.UtcNow,
                       Recipients = TestRecipients1()
                   };
        }
	}
}