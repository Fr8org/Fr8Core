using System;
using System.Linq;
using StructureMap;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using NUnit.Framework;
using terminalAtlassianTests.Fixtures;
using Data.Interfaces.Manifests;
using UtilitiesTesting;
using TerminalBase.Infrastructure;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using terminalAtlassian.Activities;

namespace terminalDropboxTests.Actions
{
    [TestFixture]
    [Category("terminalAtlassianTests1")]
    public class Get_Jira_Issue_v1Tests : BaseTest
    {
        private Get_Jira_Issue_v1 _get_Jira_Issue_v1;

        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();
            var restfulServiceClient = new Mock<IRestfulServiceClient>();
            restfulServiceClient.Setup(r => r.GetAsync<PayloadDTO>(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(Task.FromResult(FixtureData.FakePayloadDTO));
            ObjectFactory.Configure(cfg => cfg.For<IRestfulServiceClient>().Use(restfulServiceClient.Object));

            _get_Jira_Issue_v1 = new Get_Jira_Issue_v1();
            _get_Jira_Issue_v1.HubCommunicator.Configure("terminalAtlassian");
        }

        [Test]
        public void Run_ReturnsPayloadDTO()
        {
            //Arrange
            var curActivityDO = FixtureData.GetJiraIssueTestActivityDO1();
            var container = FixtureData.TestContainer();
            //Act
            var payloadDTOResult = _get_Jira_Issue_v1.Run(curActivityDO, container.Id, FixtureData.JiraAuthorizationToken()).Result;

            //Assert
            var jiraIssue = JsonConvert.DeserializeObject<StandardPayloadDataCM>(payloadDTOResult.CrateStorage.Crates[1].Contents.ToString());
            Assert.AreEqual("FR-1245", jiraIssue.PayloadObjects[0].PayloadObject.First(x => x.Key == "Key").Value);
            Assert.AreEqual("admin", jiraIssue.PayloadObjects[0].PayloadObject.First(x => x.Key == "Reporter").Value);

        }
    }
}
