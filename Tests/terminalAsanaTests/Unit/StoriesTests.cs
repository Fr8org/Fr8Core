using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using terminalAsana.Asana;
using terminalAsana.Asana.Services;
using terminalAsana.Interfaces;
using System.Web;

namespace terminalAsanaTests.Unit
{
    [Category("asanaSDK")]
    class StoriesTests : BaseAsanaTest
    {
        private IAsanaStories _asanaStoriesService;
        private IAsanaOAuth _asanaOAuth;
        private IAsanaOAuthCommunicator _communicator;

        [TestFixtureSetUp]
        public void StartUp()
        {
            _asanaOAuth = new AsanaOAuthService(_restClient, _parameters);
            _communicator = new AsanaCommunicatorService(_asanaOAuth, _restClient);
            _asanaStoriesService = new Stories(_communicator, _parameters);
        }

        [Test]
        public async Task Should_Post_Comment()
        {
            var taskId = "1";
            var comment = "comnt Asna";
            var actual = "";

            _restClientMock.Setup(x => x.PostAsync<JObject>(
                It.Is<Uri>(y => y.PathAndQuery.Equals(new Uri(_parameters.StoriesUrl).PathAndQuery)),
                It.IsAny<HttpContent>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()
                ))
                .Callback<Uri, HttpContent, string, Dictionary<string, string>>(async (x, y, z, d)  =>  
                {
                    actual = await y.ReadAsStringAsync();
                })
                .ReturnsAsync(JObject.Parse(Fixtures.FixtureData.SampleAsanaOauthTokenResponse()));

            var story = await _asanaStoriesService.PostCommentAsync(taskId,comment);

            //Assertion
            Assert.IsTrue(actual.Contains(HttpUtility.UrlEncode(comment)));
            Assert.IsNotNull(story);
            _restClientMock.Verify();
        }
    }
}
