using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;

namespace terminalSalesforceTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        private const int ActivityCount = 6;
        private const string Save_To_SalesforceDotCom_Name = "Save_To_SalesforceDotCom";
        private const string Get_Data_Activity_Name = "Get_Data";
        private const string Post_To_Chatter_Name = "Post_To_Chatter";
        private const string MailMergeFromSalesforceName = "Mail_Merge_From_Salesforce";
        private const string Monitor_Salesforce_Event_Name = "Monitor_Salesforce_Event";

        public override string TerminalName
        {
            get { return "terminalSalesforce"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, CategoryAttribute("Integration.terminalSalesforce")]
        public async Task Terminal_Salesforce_Discover()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.IsNotNull(terminalDiscoverResponse, "Terminal Salesforce discovery did not happen.");
            Assert.IsNotNull(terminalDiscoverResponse.Activities, "Salesforce terminal actions were not loaded");
            Assert.AreEqual(ActivityCount, terminalDiscoverResponse.Activities.Count,
                "Not all terminal Salesforce actions were loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Save_To_SalesforceDotCom_Name), true,
                "Action " + Save_To_SalesforceDotCom_Name + " was not loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Get_Data_Activity_Name), true,
                "Action " + Get_Data_Activity_Name + " was not loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Post_To_Chatter_Name), true,
                "Action " + Post_To_Chatter_Name + " was not loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == MailMergeFromSalesforceName), true,
               "Action " + MailMergeFromSalesforceName + " was not loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Monitor_Salesforce_Event_Name), true,
                "Action " + Monitor_Salesforce_Event_Name + " was not loaded");
        }
    }
}
