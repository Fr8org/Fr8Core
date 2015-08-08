using System;

using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
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
