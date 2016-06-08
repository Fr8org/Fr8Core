using System.Linq;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;

namespace terminalExcelTests.Integration
{
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        private const int ActivityCount = 3;
        private const string Load_Excel_File_Name = "Load_Excel_File";
        private const string SetExcelTemplate = "SetExcelTemplate";
        private const string Save_To_Excel = "Save_To_Excel";

        public override string TerminalName
        {
            get { return "terminalExcel"; }
        }

        [Test]
        public async Task Discover_Check_Returned_Activities()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.NotNull(terminalDiscoverResponse);
            Assert.AreEqual(ActivityCount, terminalDiscoverResponse.Activities.Count);
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Load_Excel_File_Name), true);
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == SetExcelTemplate), true);
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Save_To_Excel), true);
        }
    }
}
