using HealthMonitor.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class Action_Documentation_Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        [Test]
        public async Task ActionDocument_PostHelpFileName_ReturnsHelpContent()
        {
            var documentationUrl = GetTerminalDocumentationUrl();

            var response = await HttpPostAsync<string, HttpResponseMessage>(
                    documentationUrl,
                    "ExplainMonitoring"
                );

            response.EnsureSuccessStatusCode();
        }
    }   
}
