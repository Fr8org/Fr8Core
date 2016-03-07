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


            hubCommunicatorMock.Setup(h => h.GetDesignTimeFieldsByDirection(It.IsAny<Guid>(), It.IsAny<CrateDirection>(), 
                It.IsAny<AvailabilityType>(), It.IsAny<string>())).Returns(() => Task.FromResult(new FieldDescriptionsCM()));

            ObjectFactory.Container.Inject(typeof(IHubCommunicator), hubCommunicatorMock.Object);

            Mock<ISalesforceManager> salesforceIntegrationMock = Mock.Get(ObjectFactory.GetInstance<ISalesforceManager>());
            salesforceIntegrationMock.Setup(si => si.GetChatters(It.IsAny<AuthorizationTokenDO>())).Returns(
                () => Task.FromResult<IList<FieldDTO>>(new List<FieldDTO> { new FieldDTO("One", "1")}));
            salesforceIntegrationMock.Setup(si => si.PostFeedTextToChatterObject("SomeValue", "SomeValue", 
                It.IsAny<AuthorizationTokenDO>())).Returns(() => Task.FromResult(true));

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
            var stroage = ObjectFactory.GetInstance<ICrateManager>().GetStorage(result);
            Assert.AreEqual(3, stroage.Count, "Number of configuration crates not populated correctly");

            var configControlCM = stroage.CratesOfType<StandardConfigurationControlsCM>().Single();
            Assert.IsNotNull(configControlCM, "Configuration controls is not present");

            var availableChatters = stroage.CratesOfType<FieldDescriptionsCM>().Single(x => x.Label.Equals("AvailableChatters"));
            Assert.AreEqual(1, availableChatters.Content.Fields.Count, "Available chatter objects are not correct");

            Assert.AreEqual(2, configControlCM.Content.Controls.Count, "Number of configuration controls are not correct");
            Assert.IsTrue(configControlCM.Content.Controls.Any(control => control.Name.Equals("WhatKindOfChatterObject")), "WhatKindOfChatterObject DDLB is not present");
            Assert.IsTrue(configControlCM.Content.Controls.Any(control => control.Name.Equals("FeedTextItem")), "FeedTextItem is not present");
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
            var stroage = ObjectFactory.GetInstance<ICrateManager>().GetStorage(resultPayload);
            Assert.AreEqual("Success", stroage.CratesOfType<OperationalStateCM>().Single().Content.CurrentActivityResponse.Type, 
                "The Run method did not get success");
        }

        private ActivityDO SetValues(ActivityDO curActivityDO)
        {
            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(curActivityDO))
            {
                var configControls = crateStorage.CratesOfType<StandardConfigurationControlsCM>().Single();
                configControls.Content.Controls.Where(control => control.Name != null && control.Name.Equals("WhatKindOfChatterObject"))
                    .Select(control => control as DropDownList)
                    .Single()
                    .Value = "SomeValue";

                var textSource = configControls.Content.Controls.Where(control => control.Name != null && control.Name.Equals("FeedTextItem"))
                    .Select(control => control as TextSource)
                    .Single();

                textSource.ValueSource = "specific";
                textSource.TextValue = "SomeValue";
            }
            return curActivityDO;
        }
    }
}
