using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Testing.Integration;
using NUnit.Framework;
using Fr8.Infrastructure.Data.Manifests;

namespace terminalStatXTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Terminal_Discover_v1_Tests : BaseTerminalIntegrationTest
    {
        private const int ActivityCount = 3;
        private const string Monitor_Stat_Changes_Activity_Name = "Monitor_Stat_Changes";
        private const string Update_Stat_Activity_Name = "Update_Stat";
        private const string Create_Stat_Activity_Name = "Create_Stat";

        public override string TerminalName => "terminalStatX";

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, System.ComponentModel.Category("Integration.terminalStatX")]
        public async Task Terminal_StatX_Discover()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.IsNotNull(terminalDiscoverResponse, "Terminal StatX discovery did not happen.");
            Assert.IsNotNull(terminalDiscoverResponse.Activities, "StatX terminal actions were not loaded");
            Assert.AreEqual(ActivityCount, terminalDiscoverResponse.Activities.Count, "Not all terminal statX activities were loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Monitor_Stat_Changes_Activity_Name), true, "Activity " + Monitor_Stat_Changes_Activity_Name + " was not loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Update_Stat_Activity_Name), true, "Activity " + Update_Stat_Activity_Name + " was not loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Create_Stat_Activity_Name), true, "Activity " + Create_Stat_Activity_Name + " was not loaded");
        }
    }
}
