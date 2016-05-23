using HealthMonitor.Utility;
using NUnit.Framework;

namespace terminalIntegrationTests.EndToEnd
{
    [Explicit]
    public class WarehouseSearch_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        [Test]
        public void WarehouseSearch_Simple()
        {

        }
    }
}
