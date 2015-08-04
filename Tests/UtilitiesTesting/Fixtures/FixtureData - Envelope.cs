using System;
using System.Collections.Generic;

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

        public static Envelope TestEnvelope1WithGivenAccount(Account account)
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

        private static Recipients TestRecipients1()
        {
            return new Recipients
                   {
                       recipientCount = "1",
                       signers = new[]
                                 {
                                     TestSigner1()
                                 }
                   };
        }

        private static Signer TestSigner1()
        {
            return new Signer
                   {
                       recipientId = Guid.NewGuid().ToString(),
                       name = "Orkan ARI",
                       email = "hello@orkan.com",
                   };
        }

	    public static TabCollection TestTabCollection1()
	    {
	        return new TabCollection
	               {
	                   textTabs = new List<TextTab>
	                              {
	                                  TestTab1()
	                              }
	               };
	    }

        private static TextTab TestTab1()
        {
            return new TextTab
                   {
                       required = false,
                       height = 200,
                       width = 200,
                       xPosition = 200,
                       yPosition = 200,
                       name = "Amount",
                       value = "40"
                   };
        }
	}
}