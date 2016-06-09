using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;

namespace terminalSlackTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        private const int ActivityCount = 4;
        private const string Monitor_Channel_Activity_Name = "Monitor_Channel";
        private const string Publish_To_Slack_Activity_Name = "Publish_To_Slack";
        public override string TerminalName
        {
            get { return "terminalSlack"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, CategoryAttribute("Integration.terminalSlack")]
        public async Task Terminal_Slack_Discover()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.IsNotNull(terminalDiscoverResponse, "Terminal Slack discovery did not happen.");
            Assert.IsNotNull(terminalDiscoverResponse.Activities, "Slack terminal actions were not loaded");
            Assert.AreEqual(ActivityCount, terminalDiscoverResponse.Activities.Count, "Not all terminal slack actions were loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Monitor_Channel_Activity_Name), true, "Action " + Monitor_Channel_Activity_Name + " was not loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Publish_To_Slack_Activity_Name), true, "Action " + Publish_To_Slack_Activity_Name + " was not loaded");
        }
    }
}
