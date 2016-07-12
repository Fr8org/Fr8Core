using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;

namespace terminalInstagramTests.Integration
{
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        private const int InstagramActivityCount = 1;
        private const string MonitorForNewMediaPosted = "Monitor_For_New_Media_Posted";


        public override string TerminalName
        {
            get { return "terminalInstagram"; }
        }

        [Test]
        public async Task Discover_Check_Returned_Activities()
        {
            var discoverUrl = GetTerminalDiscoverUrl();
            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);
            Assert.NotNull(terminalDiscoverResponse);
            Assert.AreEqual(InstagramActivityCount, terminalDiscoverResponse.Activities.Count, "InstagramActivityCount is not equal to " + InstagramActivityCount);
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == MonitorForNewMediaPosted), "MonitorForNewMediaPosted wasn`t found");
        }
    }
}
