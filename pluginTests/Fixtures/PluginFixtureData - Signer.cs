using System;

using DocuSign.Integrations.Client;

namespace terminalTests.Fixtures
{
	public partial class PluginFixtureData
	{
		private static Signer TestSigner1()
		{
			return new Signer
					 {
						 recipientId = Guid.NewGuid().ToString(),
						 name = "Orkan ARI",
						 email = "hello@orkan.com",
					 };
		}
	}
}
