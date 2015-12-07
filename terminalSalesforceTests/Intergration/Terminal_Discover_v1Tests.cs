using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;

namespace terminalSalesforceTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseHealthMonitorTest
    {
        private const int ActionCount = 3;
        private const string Create_Account_Action_Name = "Create_Account";
        private const string Create_Contact_Action_Name = "Create_Contact";
        private const string Create_Lead_Action_Name = "Create_Lead";


        public override string TerminalName
        {
            get { return "terminalSalesforce"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, CategoryAttribute("Integration.terminalSalesforce")]
        public async void Terminal_Salesforce_Discover()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.IsNotNull(terminalDiscoverResponse, "Terminal Salesforce discovery did not happen.");
            Assert.IsNotNull(terminalDiscoverResponse.Actions, "Salesforce terminal actions were not loaded");
            Assert.AreEqual(ActionCount, terminalDiscoverResponse.Actions.Count,
                "Not all terminal Salesforce actions were loaded");
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Create_Account_Action_Name), true,
                "Action " + Create_Account_Action_Name + " was not loaded");
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Create_Contact_Action_Name), true,
                "Action " + Create_Contact_Action_Name + " was not loaded");
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Create_Lead_Action_Name), true,
                "Action " + Create_Lead_Action_Name + " was not loaded");
        }
    }
}
