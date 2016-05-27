using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalSalesforce;
using terminalSalesforce.Actions;
using terminalSalesforce.Infrastructure;
using terminalSalesforceTests.Fixtures;
using TerminalBase.Infrastructure;
using UtilitiesTesting;
using TerminalBase.Models;
using Fr8Data.Managers;
using TerminalBase.Helpers;

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

            hubCommunicatorMock.Setup(h => h.GetPayload(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(testPayloadDTO));


            hubCommunicatorMock.Setup(h => h.GetDesignTimeFieldsByDirection(It.IsAny<Guid>(), It.IsAny<CrateDirection>(), 
                It.IsAny<AvailabilityType>(), It.IsAny<string>())).Returns(() => Task.FromResult(new FieldDescriptionsCM()));

            ObjectFactory.Container.Inject(typeof(IHubCommunicator), hubCommunicatorMock.Object);

            Mock<ISalesforceManager> salesforceIntegrationMock = Mock.Get(ObjectFactory.GetInstance<ISalesforceManager>());
            salesforceIntegrationMock.Setup(si => si.GetUsersAndGroups(It.IsAny<AuthorizationToken>())).Returns(
                () => Task.FromResult<IList<FieldDTO>>(new List<FieldDTO> { new FieldDTO("One", "1")}));
            salesforceIntegrationMock.Setup(si => si.PostToChatter(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<AuthorizationToken>())).Returns(() => Task.FromResult("SomeValue"));

            postToChatter_v1 = new Post_To_Chatter_v1();
        }

        [Test, Category("terminalSalesforceTests.Post_To_Chatter_v1.Configure")]
        public async Task Configure_InitialConfig_CheckControlsCrate()
        {
            //Arrange
            var activityContext = await FixtureData.GetFileListTestActivityContext2();

            //Act
            await postToChatter_v1.Configure(activityContext);

            //Assert
            var storage = activityContext.ActivityPayload.CrateStorage;
            Assert.AreEqual(2, storage.Count, "Number of configuration crates not populated correctly");
            Assert.IsNotNull(storage.FirstCrateOrDefault<StandardConfigurationControlsCM>(), "Configuration controls crate is not found in activity storage");
            Assert.IsNotNull(storage.FirstCrateOrDefault<CrateDescriptionCM>(), "Crate with runtime crates desriptions is not found in activity storage");
        }

        [Test, Category("terminalSalesforceTests.Post_To_Chatter_v1.Run")]
        public async Task Run_Check_PayloadDTO_ForObjectData()
        {
            //Arrange
            var activityContext = await FixtureData.GetFileListTestActivityContext2();
            var executionContext = new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
            };

            //perform initial configuration
            await postToChatter_v1.Configure(activityContext);
            activityContext = SetValues(activityContext);

            //Act
            await postToChatter_v1.Run(activityContext, executionContext);

            //Assert
            var storage = executionContext.PayloadStorage;
            Assert.IsNotNull(storage.FirstCrateOrDefault<StandardPayloadDataCM>(), "Payload doesn't contain crate with posted feed Id");
        }

        private ActivityContext SetValues(ActivityContext activityContext)
        {
            activityContext.ActivityPayload.CrateStorage.UpdateControls<Post_To_Chatter_v1.ActivityUi>(x =>
            {
                x.UseUserOrGroupOption.Selected = true;
                x.UserOrGroupSelector.Value = "1";
                x.FeedTextSource.ValueSource = "specific";
                x.FeedTextSource.TextValue = "SomeValue";
            });
            return activityContext;
        }
    }
}
