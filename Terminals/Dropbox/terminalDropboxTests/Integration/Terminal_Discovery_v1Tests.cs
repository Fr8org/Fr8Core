using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;

namespace terminalDropboxTests.Integration
{
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        private const int ActivityCount = 1;
        private const string Get_File_List_Activity_Name = "Get_File_List";

        public override string TerminalName => "terminalDropbox";

        [Test, CategoryAttribute("Integration.terminalDropbox")]
        public async Task Discover_Check_Returned_Activities()
        {
            //Arrange
            var discoverUrl = GetTerminalDiscoverUrl();

            //Act
            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            //Assert
            Assert.IsNotNull(terminalDiscoverResponse, "Terminal Dropbox discovery did not happen.");
            Assert.IsNotNull(terminalDiscoverResponse.Activities, "Dropbox terminal actions were not loaded");
            Assert.AreEqual(ActivityCount, terminalDiscoverResponse.Activities.Count,
            "Not all terminal Dropbox actions were loaded");
            Assert.AreEqual(terminalDiscoverResponse.Activities.Any(a => a.Name == Get_File_List_Activity_Name), true, "Action " + Get_File_List_Activity_Name + " was not loaded");

        }
    }
}
