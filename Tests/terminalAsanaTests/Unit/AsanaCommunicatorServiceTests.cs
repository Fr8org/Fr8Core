using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using terminalAsana.Asana;
using terminalAsana.Asana.Services;
using terminalAsana.Interfaces;

namespace terminalAsanaTests.Unit
{
    [Category("asanaSDK")]
    class AsanaCommunicatorServiceTests : BaseAsanaTest
    {
        private IAsanaOAuthCommunicator _communicator;
        private IAsanaOAuth _asanaOAuth;

        [TestFixtureSetUp]
        public async Task Start()
        {
            this._asanaOAuth = await new AsanaOAuthService(_restClient, _parameters).InitializeAsync(new OAuthToken());
        }

        [Test]
        public async Task ShouldAddHeaderIfEmpty()
        {

            var originalHeaders = new Dictionary<string,string>();
            var headers = await _communicator.PrepareHeader(originalHeaders);

            this._communicator = new AsanaCommunicatorService(_asanaOAuth, _restClient);

            Assert.IsTrue(headers.Count == 1);
            Assert.IsTrue(headers.ContainsKey("Authorization"));
            Assert.IsTrue(headers.Select(x=>x.Value.Contains("Bearer")).Any());
        }

        [Test]
        public async Task ShouldAddHeaderIfAlreadyHasIt()
        {
            this._communicator = new AsanaCommunicatorService(_asanaOAuth, _restClient);

            var originalHeaders = new Dictionary<string, string>() { {"key","value"}, {"Auth","$Bearer"} };
            var headers = await _communicator.PrepareHeader(originalHeaders);

            Assert.IsTrue(headers.Count > 1);
            Assert.IsTrue(headers.ContainsKey("Authorization"));
            Assert.IsTrue(headers.Select(x => x.Value.Contains("Bearer")).Any());
        }

        [Test]
        public async Task ShouldPostWithHeader()
        {
            Dictionary<string,string> actual = null;
            _restClientMock.Setup(x => x.PostAsync<JObject>(It.IsAny<Uri>(),
                It.IsAny<HttpContent>(),
                null,
                It.IsAny<Dictionary<string, string>>()))
                .Callback<Uri, HttpContent, string, Dictionary<string,string>>((x,y,z,d)=>
                {
                    actual = d;
                })
                .ReturnsAsync(JObject.Parse(Fixtures.FixtureData.SampleAsanaOauthTokenResponse()));

            this._communicator = new AsanaCommunicatorService(_asanaOAuth, _restClient);

            var result = _communicator.PostAsync<JObject>(new Uri("http://any"), null, null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(actual.ContainsKey("Authorization"));
        }


        [Test]
        public async Task ShouldGetWithHeader()
        {
            Dictionary<string, string> actual = null;
            _restClientMock.Setup(x => x.GetAsync<JObject>(It.IsAny<Uri>(),
                null,
                It.IsAny<Dictionary<string, string>>()))
                .Callback<Uri, string, Dictionary<string, string>>((x, z, d) =>
                {
                    actual = d;
                })
                .ReturnsAsync(JObject.Parse(Fixtures.FixtureData.SampleAsanaOauthTokenResponse()));

            this._communicator = new AsanaCommunicatorService(_asanaOAuth, _restClient);

            var result = _communicator.GetAsync<JObject>(new Uri("http://any"), null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(actual.ContainsKey("Authorization"));
        }
    }
}