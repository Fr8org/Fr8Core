using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using terminalAsana.Asana;
using terminalAsana.Asana.Services;
using terminalAsana.Interfaces;

namespace terminalAsanaTests.Unit
{
    [Category("asanaSDK")]
    class WorkspacesTests : BaseAsanaTest
    {
        private IAsanaWorkspaces _asanaWorkspacesService;
        private IAsanaOAuth _asanaOAuth;
        private IAsanaOAuthCommunicator _communicator;

        [TestFixtureSetUp]
        public void StartUp()
        {
            _asanaOAuth = new AsanaOAuthService(_restClient, _parameters);
            _communicator = new AsanaCommunicatorService(_asanaOAuth, _restClient);
            _asanaWorkspacesService = new Workspaces(_communicator, _parameters);
        }

        [Test]
        public async Task Should_Get_Workspaces()
        {
            _restClientMock.Setup(x => x.GetAsync<JObject>(
                It.Is<Uri>(y => y.PathAndQuery.Equals(new Uri(_parameters.WorkspacesUrl).PathAndQuery)),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string,string>>()
                )).ReturnsAsync(JObject.Parse(Fixtures.FixtureData.SampleUsersDataResponse()));

            var workspaces = await _asanaWorkspacesService.GetAsync();

            //Assertion
            Assert.IsNotNull(workspaces);
            _restClientMock.Verify();
        }
    }
}
