using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;
using System.Linq;

namespace terminalQuickBooksTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseHealthMonitorTest
    {
        private const int ActionCount = 1;
        private const string Create_Journal_Entry_Action_Name = "Create_Journal_Entry";
        public override string TerminalName
        {
            get { return "terminalQuickBooks"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, NUnit.Framework.Category("Integration.terminalQuickBooks")]
        public async void Terminal_Slack_Discover()
        {
            var discoverUrl = GetTerminalDiscoverUrl();
            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);
            Assert.IsNotNull(terminalDiscoverResponse, "Terminal QuickBooks discovery did not happen.");
            Assert.IsNotNull(terminalDiscoverResponse.Actions, "QuickBooks terminal actions were not loaded");
            Assert.AreEqual(ActionCount, terminalDiscoverResponse.Actions.Count, "Not all terminal slack actions were loaded");
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Create_Journal_Entry_Action_Name), true, "Action " + Create_Journal_Entry_Action_Name + " was not loaded");
            Assert.AreEqual("QuickBooks", terminalDiscoverResponse.Actions[0].WebService.Name);
        }
    }
}
