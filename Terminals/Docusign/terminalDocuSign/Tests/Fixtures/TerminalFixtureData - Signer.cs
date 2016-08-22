using terminalDocuSign.Infrastructure;
using System;

namespace terminalDocuSign.Tests.Fixtures
{
    public partial class TerminalFixtureData
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