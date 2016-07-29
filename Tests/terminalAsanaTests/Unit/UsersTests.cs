using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Fr8.Testing.Unit;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using terminalAsana.Asana;
using terminalAsana.Asana.Services;
using terminalAsana.Interfaces;

namespace terminalAsanaTests.Unit
{
    [Category("asanaSDK")]
    class UsersTests: BaseAsanaTest
    {
        private IAsanaUsers _asanaUsersService;
        private IAsanaOAuth _asanaOAuth;
        private IAsanaOAuthCommunicator _communicator;

        [TestFixtureSetUp]
        public void StartUp()
        {
            _asanaOAuth = new AsanaOAuthService(_restClient,_parameters);
            _communicator = new AsanaCommunicatorService(_asanaOAuth,_restClient);
            _asanaUsersService = new Users(_communicator, _parameters);
        }

        [Test]
        public async Task Should_Get_Me_Info()
        {
            //wait for http call
            _restClientMock.Setup(x => x.GetAsync<JObject>(
                It.Is<Uri>(y => y.AbsoluteUri.Equals(new Uri(_parameters.UsersMeUrl).AbsoluteUri)),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()
                )).ReturnsAsync(JObject.Parse(Fixtures.FixtureData.SampleAsanaOauthTokenResponse()));

            var meInfo = await _asanaUsersService.MeAsync();

            // Assertion
            Assert.IsNotNull(meInfo);
            _restClientMock.Verify();
        }

        [Test]
        public async Task Should_Get_Users_in_Workspace()
        {
            //wait for http call
            _restClientMock.Setup(x => x.GetAsync<JObject>(
                It.Is<Uri>(y => y.AbsoluteUri.Equals(new Uri(_parameters.UsersInWorkspaceUrl).AbsoluteUri)),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()
                )).ReturnsAsync(JObject.Parse(Fixtures.FixtureData.SampleUsersDataResponse()));

            var users = await _asanaUsersService.GetUsersAsync("1");

            // Assertion
            Assert.IsNotNull(users);
            _restClientMock.Verify();
        }


        [Test]
        public async Task Should_Get_User()
        {
            //wait for http call
            _restClientMock.Setup(x => x.GetAsync<JObject>(
                It.Is<Uri>(y => y.AbsoluteUri.Equals(new Uri(_parameters.UsersUrl).AbsoluteUri)),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()
                )).ReturnsAsync(JObject.Parse(Fixtures.FixtureData.SampleAsanaOauthTokenResponse()));

            var users = await _asanaUsersService.GetUserAsync("2");

            // Assertion
            Assert.IsNotNull(users);
            _restClientMock.Verify();
        }

    }
}
