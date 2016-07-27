using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalSlack.Interfaces;
using terminalSlackTests.Fixtures;
using Fr8.Testing.Unit;
using terminalSlack.Activities;

namespace terminalSlackTests.Activities
{
    [TestFixture, Category("terminalSlack")]
    public class Monitor_Channel_v2Tests : BaseTest
    {
        private static readonly CrateManager CrateManager = new CrateManager();

        private static readonly AuthorizationToken AuthorizationToken = new AuthorizationToken { Token = "1" };

        public override void SetUp()
        {
            base.SetUp();
            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            ObjectFactory.Container.Inject(hubCommunicatorMock);
            ObjectFactory.Container.Inject(hubCommunicatorMock.Object);
            HealthMonitor_FixtureData.ConfigureHubToReturnEmptyPayload();
            var slackIntegrationMock = new Mock<ISlackIntegration>();
            slackIntegrationMock.Setup(x => x.GetChannelList(It.IsAny<string>(), It.IsAny<bool>()))
                                .Returns(Task.FromResult(new List<KeyValueDTO> { new KeyValueDTO("#channel", "1") }));
            ObjectFactory.Container.Inject(slackIntegrationMock);
            ObjectFactory.Container.Inject(slackIntegrationMock.Object);
            var slackEventManagerMock = new Mock<ISlackEventManager>();
            ObjectFactory.Container.Inject(slackEventManagerMock.Object);
            ObjectFactory.Container.Inject(slackEventManagerMock);
        }

        [Test]
        public async Task Initialize_Always_LoadsChannelList()
        {
            var activity = New<Monitor_Channel_v2>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = AuthorizationToken
            };
            await activity.Configure(activityContext);
            ObjectFactory.GetInstance<Mock<ISlackIntegration>>().Verify(x => x.GetChannelList(It.IsAny<string>(), false), "Channel list was not loaded from Slack");
        }

        [Test]
        public async Task Initialize_Always_HasEventSubscriptonCrate()
        {
            var activity = New<Monitor_Channel_v2>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = AuthorizationToken
            };
            await activity.Configure(activityContext);
            Assert.IsNotNull(activityContext.ActivityPayload.CrateStorage.FirstCrateOrDefault<EventSubscriptionCM>(), "Event subscription was not created");
        }

        [Test]
        public async Task Initialize_Always_ReportsRuntimeAvailableFields()
        {
            var activity = New<Monitor_Channel_v2>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = AuthorizationToken
            };
            await activity.Configure(activityContext);
            var currentActivityStorage = activityContext.ActivityPayload.CrateStorage;
            var runtimeCratesDescriptionCrate = currentActivityStorage.FirstCrateOrDefault<CrateDescriptionCM>();
            Assert.IsNotNull(runtimeCratesDescriptionCrate, "Runtime crates description crate was not created");
            Assert.IsTrue(runtimeCratesDescriptionCrate.Content.CrateDescriptions[0].Fields.Count > 0, "Runtime available fields were not reported");
        }

        [Test]
        public async Task Run_WhenNoMonitorOptionIsSelected_ReturnsValidationError()
        {
            var activity = New<Monitor_Channel_v2>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = AuthorizationToken
            };
            var executionContext = new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
            };
            await activity.Configure(activityContext);

            var configurationControls = activityContext.ActivityPayload.CrateStorage.FirstCrate<StandardConfigurationControlsCM>().Content;

            configurationControls.FindByNameNested<CheckBox>("MonitorChannelsOption").Selected = false;
            configurationControls.FindByNameNested<RadioButtonOption>("AllChannelsOption").Selected = false;
            
            await activity.Run(activityContext, executionContext);
            var operationalState = executionContext.PayloadStorage.FirstCrateContentOrDefault<OperationalStateCM>();

            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalState.CurrentActivityResponse.Type, "Error response was not produced when no monitor option was selected");
        }

        [Test]
        public async Task Activate_WhenConfiguredProperly_SubscribesToSlackRtmEvents()
        {
            var activity = New<Monitor_Channel_v2>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                AuthorizationToken = AuthorizationToken,
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
            };
            await activity.Configure(activityContext);
            activityContext.ActivityPayload.CrateStorage.UpdateControls<Monitor_Channel_v2.ActivityUi>(x => x.MonitorDirectMessagesOption.Selected = true);
            var planId = activityContext.ActivityPayload.RootPlanNodeId = Guid.NewGuid();
            await activity.Activate(activityContext);
            ObjectFactory.GetInstance<Mock<ISlackEventManager>>().Verify(x => x.Subscribe(It.IsAny<AuthorizationToken>(), planId.Value), "Subscription to Slack RTM was not created");
        }

        [Test]
        public async Task Run_WhenPayloadHasEventAndItDoesntPassFilters_ReturnsTerminationRequest()
        {
            var activity = New<Monitor_Channel_v2>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = AuthorizationToken
            };
            var executionContext = new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
            };

            await activity.Configure(activityContext);

            activity = New<Monitor_Channel_v2>();

            activityContext.ActivityPayload.CrateStorage.UpdateControls<Monitor_Channel_v2.ActivityUi>(x =>
                                                                          {
                                                                              x.MonitorDirectMessagesOption.Selected = true;
                                                                              x.MonitorChannelsOption.Selected = false;
                                                                          });
            HealthMonitor_FixtureData.ConfigureHubToReturnPayloadWithChannelMessageEvent();
            await activity.Run(activityContext, executionContext);
            var operationalState = executionContext.PayloadStorage.FirstCrateContentOrDefault<OperationalStateCM>();
            Assert.AreEqual(ActivityResponse.RequestTerminate.ToString(), operationalState.CurrentActivityResponse.Type, "RequestTerminate response was not produced when event didn't match monitoring options");
        }

        [Test]
        public async Task Run_WhenPayloadHasEventAndItPassFilters_ReturnsSuccess()
        {
            var activity = New<Monitor_Channel_v2>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = AuthorizationToken
            };
            var executionContext = new ContainerExecutionContext
            {
                PayloadStorage = HealthMonitor_FixtureData.GetDirectMessageEventPayload()
            };
            await activity.Configure(activityContext);
            activityContext.ActivityPayload.CrateStorage.UpdateControls<Monitor_Channel_v2.ActivityUi>(x =>
            {
                x.MonitorDirectMessagesOption.Selected = true;
                x.MonitorChannelsOption.Selected = false;
            });
            await activity.Run(activityContext, executionContext);
            var operationalState = executionContext.PayloadStorage.FirstCrateContentOrDefault<OperationalStateCM>();
            Assert.AreEqual(ActivityResponse.Success.ToString(), operationalState.CurrentActivityResponse.Type, "RequestTerminate response was not produced when event didn't match monitoring options");
            Assert.IsNotNull(executionContext.PayloadStorage.FirstCrateOrDefault<StandardPayloadDataCM>(), "Activity didn't produce crate with payload data");
        }
    }
}
