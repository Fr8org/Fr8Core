using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Fr8.Testing.Unit;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using terminalAsana.Asana;
using terminalAsana.Interfaces;
using terminalAsana.Asana.Services;
using terminalAsanaTests.Fixtures;

namespace terminalAsanaTests.Unit
{

    [Category("asanaSDK")]
    class AsanaOAuthServiceTests :BaseAsanaTest
    {
        [Test]
        public async Task Should_Initialize_Itself()
        {
            var asanaOAuth = await new AsanaOAuthService(this._restClient, this._parameters).InitializeAsync(new OAuthToken());

            Assert.IsTrue(asanaOAuth.OAuthToken.ExpirationDate > DateTime.UtcNow && asanaOAuth.OAuthToken.ExpirationDate < DateTime.UtcNow.AddSeconds(3601));
            Assert.IsNotEmpty(asanaOAuth.OAuthToken.AccessToken);
            Assert.IsNotEmpty(asanaOAuth.OAuthToken.RefreshToken);
        }

        [Test]
        public async Task Should_Refresh_Token_If_It_Expired_With_Call_To_Asana_Server()
        {
            var tokenData = FixtureData.SampleAuthorizationToken();
            tokenData.AdditionalAttributes = DateTime.Parse(tokenData.AdditionalAttributes).AddHours(-12).ToString("O");

            var asanaOAuth = await new AsanaOAuthService(_restClient, _parameters).InitializeAsync(new OAuthToken());

            // Assertion
            _restClientMock.Verify(x => x.PostAsync<JObject>(It.IsAny<Uri>(),It.IsAny<HttpContent>(),null,null));

            Assert.IsTrue(asanaOAuth.OAuthToken.ExpirationDate > DateTime.UtcNow && asanaOAuth.OAuthToken.ExpirationDate < DateTime.UtcNow.AddSeconds(3601));
            Assert.IsNotEmpty(asanaOAuth.OAuthToken.AccessToken);
        }
    }
}
