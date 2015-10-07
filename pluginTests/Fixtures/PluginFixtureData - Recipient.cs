using DocuSign.Integrations.Client;

namespace terminalTests.Fixtures
{
	public partial class PluginFixtureData
	{
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
	}
}
