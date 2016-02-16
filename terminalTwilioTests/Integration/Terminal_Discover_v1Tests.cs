using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace terminalTwilioTests.Integration
{
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalTwilio"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, CategoryAttribute("Integration.terminalTwilio")]
        public async Task Terminal_Twilio_Discover()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.IsNotNull(terminalDiscoverResponse, "Terminal Twilio discovery did not happen.");
            Assert.IsNotNull(terminalDiscoverResponse.Actions, "Twilio terminal actions were not loaded");
            Assert.AreEqual(1, terminalDiscoverResponse.Actions.Count, "Not all terminal twilio actions were loaded");
            Assert.AreEqual("terminalTwilio", terminalDiscoverResponse.Definition.Name, "Definition terminalTwilio not found.");

            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == "Send_Via_Twilio"), true, "Action " + "Send_Via_Twilio" + " was not loaded");
        }
    }
}
