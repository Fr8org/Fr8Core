using System.Linq;
using System.Threading.Tasks;
using Fr8.Testing.Integration;
using Fr8.Infrastructure.Data.Manifests;
using NUnit.Framework;

namespace terminalExcelTests.Integration
{
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        private const int ActivityCount = 3;
        private const string Load_Excel_File_Name = "Load_Excel_File";
        private const string Set_Excel_Template = "Set_Excel_Template";
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
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Set_Excel_Template), true);
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Save_To_Excel), true);
        }
    }
}
