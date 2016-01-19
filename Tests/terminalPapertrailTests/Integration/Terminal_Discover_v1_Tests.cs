
using System.Runtime.Remoting;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;

namespace terminalPapertrailTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Terminal_Discover_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalPapertrail"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalPapertrail")]
        public async void Terminal_Papertrail_Discover_()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var papertrailTerminalDiscoveryResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.IsNotNull(papertrailTerminalDiscoveryResponse, "Terminal Papertrail discovery did not happen.");
            Assert.IsNotNull(papertrailTerminalDiscoveryResponse.Actions, "Papertrail terminal does not have the write to log action.");
            Assert.AreEqual(1, papertrailTerminalDiscoveryResponse.Actions.Count, "Papertrail terminal does not have the write to log action.");
            Assert.AreEqual("Write_To_Log", papertrailTerminalDiscoveryResponse.Actions[0].Name, "Name of the write to log action is wrong.");
        }
    }
}
