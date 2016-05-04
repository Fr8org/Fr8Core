using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalSlack.Interfaces;
using terminalSlackTests.Fixtures;
using TerminalBase.Infrastructure;
using UtilitiesTesting;

namespace terminalSlackTests.Activities
{
    [TestFixture, Category("terminalSlack")]
    public class Monitor_Channel_v2Tests : BaseTest
    {
        private static readonly CrateManager CrateManager = new CrateManager();

        private static readonly AuthorizationTokenDO AuthorizationToken = new AuthorizationTokenDO { Token = "1" };

        public override void SetUp()
        {
            base.SetUp();
            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            ObjectFactory.Container.Inject(hubCommunicatorMock);
            ObjectFactory.Container.Inject(hubCommunicatorMock.Object);
            HealthMonitor_FixtureData.ConfigureHubToReturnEmptyPayload();
            var slackIntegrationMock = new Mock<ISlackIntegration>();
            slackIntegrationMock.Setup(x => x.GetChannelList(It.IsAny<string>(), It.IsAny<bool>()))
                                .Returns(Task.FromResult(new List<FieldDTO> { new FieldDTO("#channel", "1") }));
            ObjectFactory.Container.Inject(slackIntegrationMock);
            ObjectFactory.Container.Inject(slackIntegrationMock.Object);
            var slackEventManagerMock = new Mock<ISlackEventManager>();
            ObjectFactory.Container.Inject(slackEventManagerMock.Object);
            ObjectFactory.Container.Inject(slackEventManagerMock);
        }

        [Test]
        public async Task Initialize_Always_LoadsChannelList()
        {
            var activity = new Monitor_Channel_v2();
            await activity.Configure(new ActivityDO(), AuthorizationToken);
            ObjectFactory.GetInstance<Mock<ISlackIntegration>>().Verify(x => x.GetChannelList(It.IsAny<string>(), false), "Channel list was not loaded from Slack");
        }

        [Test]
        public async Task Initialize_Always_HasEventSubscriptonCrate()
        {
            var activity = new Monitor_Channel_v2();
            var currentActivity = await activity.Configure(new ActivityDO(), AuthorizationToken);
            Assert.IsNotNull(CrateManager.GetStorage(currentActivity).FirstCrateOrDefault<EventSubscriptionCM>(), "Event subscription was not created");
        }

        [Test]
        public async Task Initialize_Always_ReportsRuntimeAvailableFields()
        {
            var activity = new Monitor_Channel_v2();
            var currentActivity = await activity.Configure(new ActivityDO(), AuthorizationToken);
            var currentActivityStorage = CrateManager.GetStorage(currentActivity);
            var runtimeCratesDescriptionCrate = currentActivityStorage.FirstCrateOrDefault<CrateDescriptionCM>();
            Assert.IsNotNull(runtimeCratesDescriptionCrate, "Runtime crates description crate was not created");
            Assert.IsTrue(runtimeCratesDescriptionCrate.Content.CrateDescriptions[0].Fields.Count > 0, "Runtime available fields were not reported");
        }

        [Test]
        public async Task Run_WhenNoMonitorOptionIsSelected_ReturnsError()
        {
            var activity = new Monitor_Channel_v2();
            var currentActivity = await activity.Configure(new ActivityDO(), AuthorizationToken);
            var payload = await activity.Run(currentActivity, Guid.Empty, AuthorizationToken);
            var operationalState = CrateManager.GetOperationalState(payload);
            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalState.CurrentActivityResponse.Type, "Error response was not produced when no monitor option was selected");
        }

        [Test]
        public async Task Run_WhenConfiguredProperly_SubscribesToSlackRtmEvents()
        {
            var activity = new Monitor_Channel_v2();
            var currentActivity = await activity.Configure(new ActivityDO(), AuthorizationToken);
            currentActivity.UpdateControls<Monitor_Channel_v2.ActivityUi>(x => x.MonitorDirectMessagesOption.Selected = true);
            var planId = currentActivity.RootPlanNodeId = Guid.NewGuid();
            await activity.Run(currentActivity, Guid.Empty, AuthorizationToken);
            ObjectFactory.GetInstance<Mock<ISlackEventManager>>().Verify(x => x.Subscribe(It.IsAny<AuthorizationTokenDO>(), planId.Value), "Subscription to Slack RTM was not created");
        }

        [Test]
        public async Task Run_WhenPayloadHasEventAndItDoesntPassFilters_ReturnsTerminationRequest()
        {
            var activity = new Monitor_Channel_v2();
            var currentActivity = await activity.Configure(new ActivityDO(), AuthorizationToken);
            currentActivity = currentActivity.UpdateControls<Monitor_Channel_v2.ActivityUi>(x =>
                                                                          {
                                                                              x.MonitorDirectMessagesOption.Selected = true;
                                                                              x.MonitorChannelsOption.Selected = false;
                                                                          });
            HealthMonitor_FixtureData.ConfigureHubToReturnPayloadWithChannelMessageEvent();
            var payload = await activity.Run(currentActivity, Guid.Empty, AuthorizationToken);
            var operationalState = CrateManager.GetOperationalState(payload);
            Assert.AreEqual(ActivityResponse.RequestTerminate.ToString(), operationalState.CurrentActivityResponse.Type, "RequestTerminate response was not produced when event didn't match monitoring options");
        }

        [Test]
        public async Task Run_WhenPayloadHasEventAndItPassFilters_ReturnsSuccess()
        {
            var activity = new Monitor_Channel_v2();
            var currentActivity = await activity.Configure(new ActivityDO(), AuthorizationToken);
            currentActivity = currentActivity.UpdateControls<Monitor_Channel_v2.ActivityUi>(x =>
        {
                x.MonitorDirectMessagesOption.Selected = true;
                x.MonitorChannelsOption.Selected = false;
            });
            HealthMonitor_FixtureData.ConfigureHubToReturnPayloadWithDirectMessageEvent();
            var payload = await activity.Run(currentActivity, Guid.Empty, AuthorizationToken);
            var operationalState = CrateManager.GetOperationalState(payload);
            Assert.AreEqual(ActivityResponse.Success.ToString(), operationalState.CurrentActivityResponse.Type, "RequestTerminate response was not produced when event didn't match monitoring options");
            Assert.IsNotNull(CrateManager.GetStorage(payload).FirstCrateOrDefault<StandardPayloadDataCM>(), "Activity didn't produce crate with payload data");
        }
    }
}
