using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;

namespace terminalExcelTests.Integration
{
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        private const int ActionCount = 1;
        private const string Load_Excel_File_Name = "Load_Excel_File";
       
        public override string TerminalName
        {
            get { return "terminalExcel"; }
        }

        [Test]
        public async Task Discover_Check_Returned_Actions()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.NotNull(terminalDiscoverResponse);
            Assert.AreEqual(ActionCount, terminalDiscoverResponse.Actions.Count);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Load_Excel_File_Name), true);
        }
    }
}
