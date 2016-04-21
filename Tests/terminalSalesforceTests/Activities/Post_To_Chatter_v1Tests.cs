using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerminalBase.Infrastructure;
using terminalSalesforce;
using terminalSalesforce.Actions;
using terminalSalesforce.Infrastructure;
using terminalSalesforceTests.Fixtures;
using UtilitiesTesting;

namespace terminalSalesforceTests.Actions
{
    [TestFixture]
    [Category("terminalSalesforceTests")]
    public class Post_To_Chatter_v1Tests : BaseTest
    {
        private Post_To_Chatter_v1 postToChatter_v1;

        public override void SetUp()
        {
            base.SetUp();

            TerminalBootstrapper.ConfigureTest();
            TerminalSalesforceStructureMapBootstrapper.ConfigureDependencies(TerminalSalesforceStructureMapBootstrapper.DependencyType.TEST);

            PayloadDTO testPayloadDTO = new PayloadDTO(new Guid());

            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(testPayloadDTO))
            {
                crateStorage.Add(Crate.FromContent("Operational Status", new OperationalStateCM()));
            }

            Mock<IHubCommunicator> hubCommunicatorMock = new Mock<IHubCommunicator>(MockBehavior.Default);

            hubCommunicatorMock.Setup(h => h.GetPayload(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(testPayloadDTO));


            hubCommunicatorMock.Setup(h => h.GetDesignTimeFieldsByDirection(It.IsAny<ActivityDO>(), It.IsAny<CrateDirection>(), 
                It.IsAny<AvailabilityType>(), It.IsAny<string>())).Returns(() => Task.FromResult(new FieldDescriptionsCM()));

            ObjectFactory.Container.Inject(typeof(IHubCommunicator), hubCommunicatorMock.Object);

            Mock<ISalesforceManager> salesforceIntegrationMock = Mock.Get(ObjectFactory.GetInstance<ISalesforceManager>());
            salesforceIntegrationMock.Setup(si => si.GetUsersAndGroups(It.IsAny<AuthorizationTokenDO>())).Returns(
                () => Task.FromResult<IList<FieldDTO>>(new List<FieldDTO> { new FieldDTO("One", "1")}));
            salesforceIntegrationMock.Setup(si => si.PostFeedTextToChatterObject(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<AuthorizationTokenDO>())).Returns(() => Task.FromResult("SomeValue"));

            postToChatter_v1 = new Post_To_Chatter_v1();
        }

        [Test, Category("terminalSalesforceTests.Post_To_Chatter_v1.Configure")]
        public async Task Configure_InitialConfig_CheckControlsCrate()
        {
            //Arrange
            var activityDO = FixtureData.PostToChatterTestActivityDO1();

            //Act
            var result = await postToChatter_v1.Configure(activityDO, await FixtureData.Salesforce_AuthToken());

            //Assert
            var storage = ObjectFactory.GetInstance<ICrateManager>().GetStorage(result);
            Assert.AreEqual(2, storage.Count, "Number of configuration crates not populated correctly");
            Assert.IsNotNull(storage.FirstCrateOrDefault<StandardConfigurationControlsCM>(), "Configuration controls crate is not found in activity storage");
            Assert.IsNotNull(storage.FirstCrateOrDefault<CrateDescriptionCM>(), "Crate with runtime crates desriptions is not found in activity storage");
        }

        [Test, Category("terminalSalesforceTests.Post_To_Chatter_v1.Run")]
        public async Task Run_Check_PayloadDTO_ForObjectData()
        {
            //Arrange
            var authToken = await FixtureData.Salesforce_AuthToken();
            var activityDO = FixtureData.PostToChatterTestActivityDO1();
            
            //perform initial configuration
            activityDO = await postToChatter_v1.Configure(activityDO, authToken);
            activityDO = SetValues(activityDO);

            //Act
            var resultPayload = await postToChatter_v1.Run(activityDO, new Guid(), authToken);

            //Assert
            var storage = ObjectFactory.GetInstance<ICrateManager>().GetStorage(resultPayload);
            Assert.IsNotNull(storage.FirstCrateOrDefault<StandardPayloadDataCM>(), "Payload doesn't contain crate with posted feed Id");
        }

        private ActivityDO SetValues(ActivityDO curActivityDO)
        {
            curActivityDO.UpdateControls<Post_To_Chatter_v1.ActivityUi>(x =>
            {
                x.UseUserOrGroupOption.Selected = true;
                x.UserOrGroupSelector.Value = "1";
                x.FeedTextSource.ValueSource = "specific";
                x.FeedTextSource.TextValue = "SomeValue";
            });
            return curActivityDO;
        }
    }
}
