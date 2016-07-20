using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Testing.Integration;
using NUnit.Framework;
using Fr8.Infrastructure.Data.Manifests;

namespace terminalAsanaTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Terminal_Discover_v1_Tests : BaseTerminalIntegrationTest
    {
        private const int ActivityCount = 2;
        private const string Get_Tasks_Activity_Name = "Get_Tasks";
        private const string Post_Comment_Activity_Name = "Post_Comment";

        public override string TerminalName => "terminalAsana";

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, System.ComponentModel.Category("Integration.terminalAsana")]
        public async Task Terminal_Asana_Discover()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.IsNotNull(terminalDiscoverResponse, "Terminal Asana discovery did not happen.");
            Assert.IsNotNull(terminalDiscoverResponse.Activities, "Terminal Asana actions were not loaded");
            Assert.AreEqual(ActivityCount, terminalDiscoverResponse.Activities.Count, "Not all terminal Asana activities were loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Get_Tasks_Activity_Name), true, "Activity " + Get_Tasks_Activity_Name + " was not loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Post_Comment_Activity_Name), true, "Activity " + Post_Comment_Activity_Name + " was not loaded");
        }
    }
}
