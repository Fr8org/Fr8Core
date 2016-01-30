using System.Linq;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;

namespace terminalYammerTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        private const int ActionCount = 1;
        private const string Post_To_Yammer_Action_Name = "Post_To_Yammer";
        public override string TerminalName
        {
            get { return "terminalYammer"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, CategoryAttribute("Integration.terminalYammer")]
        public async void Terminal_Yammer_Discover()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.IsNotNull(terminalDiscoverResponse, "Terminal Yammer discovery did not happen.");
            Assert.IsNotNull(terminalDiscoverResponse.Actions, "Yammer terminal activity was not loaded");
            Assert.AreEqual(ActionCount, terminalDiscoverResponse.Actions.Count, "Not all terminal Yammer activity was loaded");
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Post_To_Yammer_Action_Name), true, "Action " + Post_To_Yammer_Action_Name + " was not loaded");
        }
    }
}
