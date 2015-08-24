using System;
using Data.Entities;
using DocuSign.Integrations.Client;
using Data.Wrappers;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
		public static EnvelopeDO TestEnvelope1()
		{
			return new EnvelopeDO
			{
				DocusignEnvelopeId = "21",
				EnvelopeStatus = EnvelopeDO.EnvelopeState.Any
			};
		}

		public static DocuSignEnvelope TestEnvelope2(Account account)
		{
			// create envelope object and assign login info
			return new DocuSignEnvelope
            {
				// assign account info from above
				Login = account,
				// "sent" to send immediately, "created" to save envelope as draft
				Status = "created",
				Created = DateTime.UtcNow,
				Recipients = TestRecipients1()
			};
		}

	    public static string TestTemplateId = "b5abd63a-c12c-4856-b9f4-989200e41a6f";
	}
}