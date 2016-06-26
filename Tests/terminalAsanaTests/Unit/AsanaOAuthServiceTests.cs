using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Models;
using Fr8.Testing.Unit;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using terminalAsana;
using terminalAsana.Asana;
using terminalAsanaTests.Fixtures;

namespace terminalAsanaTests.Unit
{
    class AsanaOAuthServiceTests
    {
        private IRestfulServiceClient _restClient;
        private Mock<IRestfulServiceClient> _restClientMock;

        [SetUp]
        public void Startup()
        {
            _restClientMock = new Mock<IRestfulServiceClient>();
            _restClient = _restClientMock.Object;
        }

        [Test]
        public void Should_Initialize_Itself()
        {
            var tokenData = FixtureData.SampleAuthorizationToken();

            var asanaOAuth = new AsanaOAuthService(_restClient).Initialize(tokenData);

            Assert.IsTrue(asanaOAuth.OAuthToken.ExpirationDate > DateTime.UtcNow && asanaOAuth.OAuthToken.ExpirationDate < DateTime.UtcNow.AddSeconds(3601));
            Assert.IsNotEmpty(asanaOAuth.OAuthToken.Token);
            Assert.IsNotEmpty(asanaOAuth.OAuthToken.RefreshToken);
        }

        [Test]
        public void Should_Refresh_Token_If_It_Expired()
        {
            _restClientMock.Setup(
                x => x.PostAsync<JObject>(  It.IsAny<Uri>(), 
                                            It.IsAny<HttpContent>(),
                                            It.IsAny<string>(),
                                            It.IsAny<Dictionary<string, string>>()))

                       .ReturnsAsync(JObject.Parse(FixtureData.SampleAsanaOauthTokenResponse()));

            var tokenData = FixtureData.SampleAuthorizationToken();
            tokenData.AdditionalAttributes = DateTime.Parse(tokenData.AdditionalAttributes).AddHours(-12).ToString("O");

            var asanaOAuth = new AsanaOAuthService(_restClient).Initialize(tokenData);

            _restClientMock.Verify(x => x.PostAsync(It.IsAny<Uri>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()));

            Assert.IsTrue(asanaOAuth.OAuthToken.ExpirationDate > DateTime.UtcNow && asanaOAuth.OAuthToken.ExpirationDate < DateTime.UtcNow.AddSeconds(3601));
            Assert.IsNotEmpty(asanaOAuth.OAuthToken.Token);
        }
    }
}
