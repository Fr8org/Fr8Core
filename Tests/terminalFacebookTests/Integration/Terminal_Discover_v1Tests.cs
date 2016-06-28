using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;

namespace terminalFacebookTests.Integration
{
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        private const int FacebookActivityCount = 2;
        private const string PostToTimeline = "Post_To_Timeline";
        private const string MonitorFeedPosts = "Monitor_Feed_Posts";


        public override string TerminalName
        {
            get { return "terminalFacebook"; }
        }

        [Test]
        public async Task Discover_Check_Returned_Activities()
        {
            var discoverUrl = GetTerminalDiscoverUrl();
            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);
            Assert.NotNull(terminalDiscoverResponse);
            Assert.AreEqual(FacebookActivityCount, terminalDiscoverResponse.Activities.Count, "FacebookActivityCount is not equal to " + FacebookActivityCount);
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == PostToTimeline), "PostToTimeline wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == MonitorFeedPosts), "MonitorFeedPosts wasn`t found");
        }
    }
}
