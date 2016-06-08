using System.Linq;
using HealthMonitor.Utility;
using NUnit.Framework;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.Manifests;

namespace terminalQuickBooksTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Terminal_Discover_v1_Tests : BaseTerminalIntegrationTest
    {
        private const int ActivityCount = 1;
        private const string Create_Journal_Entry = "Create_Journal_Entry";
      //  private const string Convert_TableData_To_AccountingTransactions = "Convert_TableData_To_AccountingTransactions";

        public override string TerminalName => "terminalQuickBooks";

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalQuickBooks")]
        public async Task Quickbooks_Discover()
        {
            var discoverUrl = GetTerminalDiscoverUrl();
            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);
            Assert.IsNotNull(terminalDiscoverResponse, "Terminal QuickBooks discovery did not happen.");
            Assert.IsNotNull(terminalDiscoverResponse.Activities, "QuickBooks terminal actions were not loaded");
            Assert.AreEqual(ActivityCount, terminalDiscoverResponse.Activities.Count,
                "Not all terminal QuickBooks actions were loaded");
            //Activity Create_Journal_Entry
            Assert.True(terminalDiscoverResponse.Activities.Any(a => a.Name == Create_Journal_Entry), 
                "Activity " + Create_Journal_Entry + " was not loaded");
            Assert.AreEqual("QuickBooks", terminalDiscoverResponse.Activities[0].WebService.Name, 
                "No WebService set for activity " + Create_Journal_Entry);
            //Activity Convert_TableData_To_AccountingTransactions
            //Assert.True(terminalDiscoverResponse.Activities.Any(a => a.Name == Convert_TableData_To_AccountingTransactions),
             //   "Activity " + Convert_TableData_To_AccountingTransactions + " was not loaded");
            //Assert.AreEqual("QuickBooks", terminalDiscoverResponse.Activities[1].WebService.Name, 
             //   "No WebService set for activity " + Convert_TableData_To_AccountingTransactions);
        }
    }
}
