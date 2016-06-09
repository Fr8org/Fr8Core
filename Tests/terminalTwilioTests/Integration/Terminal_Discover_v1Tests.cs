using System.Linq;
using Fr8.Testing.Integration;
using NUnit.Framework;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;

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
            Assert.IsNotNull(terminalDiscoverResponse.Activities, "Twilio terminal actions were not loaded");
            Assert.AreEqual(1, terminalDiscoverResponse.Activities.Count, "Not all terminal twilio actions were loaded");
            Assert.AreEqual("terminalTwilio", terminalDiscoverResponse.Definition.Name, "Definition terminalTwilio not found.");
            Assert.AreEqual("Twilio", terminalDiscoverResponse.Definition.Label, "Definition Label terminalTwilio not found.");

            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == "Send_Via_Twilio"), true, "Action " + "Send_Via_Twilio" + " was not loaded");
        }
    }
}
