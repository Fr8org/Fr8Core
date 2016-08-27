using System.Linq;
using Fr8.Testing.Integration;
using NUnit.Framework;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;

namespace terminalGoogleTests.Integration
{
    /// <summary>
    /// Terminal Google Discover v1 test
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Terminal_Discover_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalGoogle";

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async Task Terminal_Google_Discover()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var googleTerminalDiscoveryResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.IsNotNull(googleTerminalDiscoveryResponse, "Terminal Google discovery did not happen.");
            Assert.IsNotNull(googleTerminalDiscoveryResponse.Activities, "Google terminal does not have actions.");
            Assert.AreEqual(5, googleTerminalDiscoveryResponse.Activities.Count, "Google terminal expected 5 actions.");
            Assert.AreEqual("terminalGoogle", googleTerminalDiscoveryResponse.Definition.Name);
            Assert.AreEqual("Google", googleTerminalDiscoveryResponse.Definition.Label);
            Assert.AreEqual(googleTerminalDiscoveryResponse.Activities.Any(a => a.Name == "Get_Google_Sheet_Data"), true, "Action Get_Google_Sheet_Data was not loaded");
            Assert.AreEqual(googleTerminalDiscoveryResponse.Activities.Any(a => a.Name == "Monitor_Form_Responses"), true, "Action Monitor_Form_Responses was not loaded");
            Assert.AreEqual(googleTerminalDiscoveryResponse.Activities.Any(a => a.Name == "Save_To_Google_Sheet"), true, "Action Save_To_Google_Sheet was not loaded");
            Assert.AreEqual(googleTerminalDiscoveryResponse.Activities.Any(a => a.Name == "Monitor_Gmail_Inbox"), true, "Action Monitor_Gmail_Inbox was not loaded");
            Assert.AreEqual(googleTerminalDiscoveryResponse.Activities.Any(a => a.Name == "Monitor_Google_Spreadsheet_Changes"), true, "Action Monitor_Google_Spreadsheet_Changes was not loaded");
            //Check Activities Categories
            Assert.True(googleTerminalDiscoveryResponse.Activities.Single(a => a.Name == "Get_Google_Sheet_Data").Categories.Any(x => x.Id == ActivityCategories.ReceiveId), "Activity Get_Google_Sheet_Data is not of Category Receivers");
            Assert.True(googleTerminalDiscoveryResponse.Activities.Single(a => a.Name == "Monitor_Form_Responses").Categories.Any(x => x.Id == ActivityCategories.MonitorId), "Activity Monitor_Form_Responses is not of Category Monitors");
            Assert.True(googleTerminalDiscoveryResponse.Activities.Single(a => a.Name == "Save_To_Google_Sheet").Categories.Any(x => x.Id == ActivityCategories.ForwardId), "Activity Save_To_Google_Sheet is not of Category Forwarders");
            Assert.True(googleTerminalDiscoveryResponse.Activities.Single(a => a.Name == "Monitor_Gmail_Inbox").Categories.Any(x => x.Id == ActivityCategories.MonitorId), "Activity Monitor_Gmail_Inbox is not of Category Monitors");
            Assert.True(googleTerminalDiscoveryResponse.Activities.Single(a => a.Name == "Monitor_Google_Spreadsheet_Changes").Categories.Any(x => x.Id == ActivityCategories.MonitorId), "Activity Monitor_Gmail_Inbox is not of Category Monitors");
        }
    }
}
