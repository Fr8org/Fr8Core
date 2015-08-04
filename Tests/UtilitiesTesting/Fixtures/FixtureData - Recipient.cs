using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
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
