using System.Linq;
using Fr8.Testing.Integration;
using NUnit.Framework;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;

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
            Assert.IsNotNull(terminalDiscoverResponse.Activities, "Slack terminal actions were not loaded");
            Assert.AreEqual(1, terminalDiscoverResponse.Activities.Count, "Not all terminal slack actions were loaded");
            Assert.AreEqual("terminalSendGrid", terminalDiscoverResponse.Definition.Name, "Definition terminalSendGrid not found.");
            Assert.AreEqual("SendGrid", terminalDiscoverResponse.Definition.Label, "Definition Label for terminalSendGrid not found.");

            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == "Send_Email_Via_SendGrid"), true, "Action " + "Send_Email_Via_SendGrid" + " was not loaded");
        }
    }
}
