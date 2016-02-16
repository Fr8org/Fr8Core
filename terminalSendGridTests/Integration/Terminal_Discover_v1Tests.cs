using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace terminalSendGridTests.Integration
{
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalSendGrid"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, CategoryAttribute("Integration.terminalSendGrid")]
        public async Task Terminal_SendGrid_Discover()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.IsNotNull(terminalDiscoverResponse, "Terminal Slack discovery did not happen.");
            Assert.IsNotNull(terminalDiscoverResponse.Actions, "Slack terminal actions were not loaded");
            Assert.AreEqual(1, terminalDiscoverResponse.Actions.Count, "Not all terminal slack actions were loaded");
            Assert.AreEqual("terminalSendGrid", terminalDiscoverResponse.Definition.Name, "Definition terminalSendGrid not found.");

            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == "SendEmailViaSendGrid"), true, "Action " + "SendEmailViaSendGrid" + " was not loaded");
        }
    }
}
