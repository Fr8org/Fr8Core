using DocuSign.Integrations.Client;
using System;

namespace pluginDocuSign.Tests.Fixtures
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