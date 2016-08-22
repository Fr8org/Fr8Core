using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using terminalAsana.Asana;
using terminalAsana.Asana.Entities;
using terminalAsana.Asana.Services;
using terminalAsana.Interfaces;

namespace terminalAsanaTests.Unit
{
    [Category("asanaSDK")]
    class TasksTests : BaseAsanaTest
    {
        private IAsanaTasks _asanaTasksService;
        private IAsanaOAuth _asanaOAuth;
        private IAsanaOAuthCommunicator _communicator;

        [TestFixtureSetUp]
        public void StartUp()
        {
            _asanaOAuth = new AsanaOAuthService(_restClient, _parameters);
            _communicator = new AsanaCommunicatorService(_asanaOAuth, _restClient);
            _asanaTasksService = new Tasks(_communicator, _parameters);
        }

        [Test]
        public async Task Should_Get_Tasks_WithProjectIdRegardlessWorkspaceAndUserId()
        {
            var projectId = "1";

            _restClientMock.Setup(x => x.GetAsync<JObject>(
                It.Is<Uri>(y => y.AbsoluteUri.Equals(new Uri(_parameters.TasksUrl+ $"?project={projectId}&").AbsoluteUri)),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()
                )).ReturnsAsync(JObject.Parse(Fixtures.FixtureData.SampleTasksDataResponse()));

            var query = new AsanaTaskQuery()
            {
                Workspace = "1",
                Assignee = "2",
                Project = projectId
            };


            var tasks = await _asanaTasksService.GetAsync(query);

            //Assertion
            Assert.IsNotNull(tasks);
            _restClientMock.Verify();
        }


        [Test]
        public async Task Should_Get_Tasks_WithWorkspaceAndUser()
        {
            _restClientMock.Setup(x => x.GetAsync<JObject>(
                It.Is<Uri>(y => y.AbsoluteUri.Equals(new Uri(_parameters.TasksUrl+ "?workspace=1&assignee=me&").AbsoluteUri)),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()
                )).ReturnsAsync(JObject.Parse(Fixtures.FixtureData.SampleTasksDataResponse()));

            var query = new AsanaTaskQuery()
            {
                Workspace = "1"                
            };


            var tasks = await _asanaTasksService.GetAsync(query);

            //Assertion
            Assert.IsNotNull(tasks);
            _restClientMock.Verify();
        }
    }
}
