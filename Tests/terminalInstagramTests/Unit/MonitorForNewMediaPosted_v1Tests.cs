using NUnit.Framework;
using Fr8.TerminalBase.Interfaces;
using Fr8.Testing.Unit;
using StructureMap;
using terminalInstagram.Interfaces;
using Moq;
using System.Threading.Tasks;
using terminalInstagram.Actions;
using Fr8.TerminalBase.Models;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Crates;
using System.Linq;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Testing.Unit.Fixtures;
using terminalInstagram.Models;
using FixtureData = terminalInstagramTests.Fixtures.FixtureData;

namespace terminalInstagramTests.Unit
{
    [TestFixture]
    public class MonitorForNewMediaPosted_v1Tests : BaseTest
    {
        private const string InstagramMediaId = "Media Id";
        private const string InstagramCaptionId = "Caption Id";
        private const string InstagramCaptionText = "Caption Text";
        private const string InstagramCaptionCreatedTimeField = "Caption Time";
        private const string InstagramImageUrl = "Image Url";
        private const string InstagramImageUrlStandardResolution = "Image Url Standard Resolution";

        public override void SetUp()
        {
            base.SetUp();
            var hubMock = new Mock<IHubCommunicator>();
            var instagramIntegrationService = new Mock<IInstagramIntegration>();
            var instagramIntegrationEventManager = new Mock<IInstagramEventManager>();
            ObjectFactory.Container.Inject(typeof(IInstagramIntegration), instagramIntegrationService.Object);
            ObjectFactory.Container.Inject(typeof(IInstagramEventManager), instagramIntegrationEventManager.Object);
            ObjectFactory.Container.Inject(hubMock);
            ObjectFactory.Container.Inject(hubMock.Object);
        }

        [Test]
        public async Task Initialize_CheckConfigControls()
        {
            var activity = New<Monitor_For_New_Media_Posted_v1>();
            var activityContext = await ConfigureMonitorForNewMediaPosted(activity);
            var configControls = activityContext.ActivityPayload.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            Assert.IsNotNull(configControls, "Monitor_For_New_Media_Posted_v1 is not initialized properly.");
            Assert.IsTrue(configControls.Content.Controls.Count == 1, "Monitor_For_New_Media_Posted_v1 configuration controls are not created correctly.");
            Assert.IsTrue(configControls.Content.Controls.Single().Type == ControlTypes.TextBlock, "Monitor_For_New_Media_Posted_v1 configuration controls are not created correctly. It should contain a single textBlock");
        }


        [Test]
        public async Task Initialize_ShouldHaveCorrectCrateStructure()
        {
            var activity = New<Monitor_For_New_Media_Posted_v1>();
            var activityContext = await ConfigureMonitorForNewMediaPosted(activity);

            var crateDescriptions = activityContext.ActivityPayload.CrateStorage.CratesOfType<CrateDescriptionCM>().FirstOrDefault();
            Assert.IsNotNull(crateDescriptions, "Monitor_For_New_Media_Posted_v1 has no crate descriptions in initial response");
            var descriptions = crateDescriptions.Content.CrateDescriptions;
            Assert.AreEqual(1, descriptions.Count, "Monitor_For_New_Media_Posted_v1 should publish single description");
            var standardPayloadDescription = descriptions.Single();
            Assert.AreEqual("Monitor Instagram Runtime Fields", standardPayloadDescription.Label, "Monitor_For_New_Media_Posted_v1 should publish crate with label Monitor Instagram Runtime Fields");
            Assert.AreEqual("Standard Payload Data", standardPayloadDescription.ManifestType, "Monitor_For_New_Media_Posted_v1 should publish Standard Payload Data");
            Assert.AreEqual(6, standardPayloadDescription.Fields.Count, "Monitor_For_New_Media_Posted_v1 should publish 6 fields");
            Assert.IsTrue(standardPayloadDescription.Fields.Any(x => x.Name == InstagramMediaId), "Monitor_For_New_Media_Posted_v1 should publish Instagram Media Id field as runtime");
            Assert.IsTrue(standardPayloadDescription.Fields.Any(x => x.Name == InstagramCaptionId), "Monitor_For_New_Media_Posted_v1 should publish Instagram Caption Id field as runtime");
            Assert.IsTrue(standardPayloadDescription.Fields.Any(x => x.Name == InstagramCaptionText), "Monitor_For_New_Media_Posted_v1 should publish Instagram Caption Text field as runtime");
            Assert.IsTrue(standardPayloadDescription.Fields.Any(x => x.Name == InstagramCaptionCreatedTimeField), "Monitor_For_New_Media_Posted_v1 should publish Instagram Caption Created Time Field field as runtime");
            Assert.IsTrue(standardPayloadDescription.Fields.Any(x => x.Name == InstagramImageUrl), "Monitor_For_New_Media_Posted_v1 should publish Instagram Image Url field as runtime");
            Assert.IsTrue(standardPayloadDescription.Fields.Any(x => x.Name == InstagramImageUrlStandardResolution), "Monitor_For_New_Media_Posted_v1 should publish Instagram Image Url Standard Resolution field as runtime");

            var eventSubscriptionCM = activityContext.ActivityPayload.CrateStorage.CratesOfType<EventSubscriptionCM>().FirstOrDefault();
            Assert.IsNotNull(eventSubscriptionCM, "Monitor_For_New_Media_Posted_v1 has no event subscription crate in initial response");
            var subscription = eventSubscriptionCM.Content;
            Assert.AreEqual(1, subscription.Subscriptions.Count, "Monitor_For_New_Media_Posted_v1 should subscribe to a single event");
            Assert.AreEqual("Standard Event Subscription", subscription.ManifestType.Type, "Monitor_For_New_Media_Posted_v1 should publish Standard Event Subscriptions");
            Assert.AreEqual("Instagram", subscription.Manufacturer, "Monitor_For_New_Media_Posted_v1 should subscribe to Instagram Manufacturer");
            Assert.IsTrue(subscription.Subscriptions.Any(x => x == "media"), "Monitor_For_New_Media_Posted_v1 should subscribe to media event");
        }

        private async Task<ActivityContext> ConfigureMonitorForNewMediaPosted(Monitor_For_New_Media_Posted_v1 activity)
        {
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = new AuthorizationToken
                {
                    Token = "1"
                }
            };
            await activity.Configure(activityContext);
            return activityContext;
        }

        private async Task<ContainerExecutionContext> RunMonitorForNewMediaPosted(Monitor_For_New_Media_Posted_v1 activity,
            ActivityContext context, ContainerExecutionContext containerExecutionContext = null)
        {
            containerExecutionContext = containerExecutionContext ?? new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
            };
            await activity.Run(context, containerExecutionContext);
            return containerExecutionContext;
        }

        [Test]
        public async Task Run_ShouldTerminateExecutionWhenEventPayloadIsNull()
        {
            var activity = New<Monitor_For_New_Media_Posted_v1>();

            var activityContext = await ConfigureMonitorForNewMediaPosted(activity);

            var containerExecutionContext = await RunMonitorForNewMediaPosted(activity, activityContext);
            var operationalStateCM = containerExecutionContext.PayloadStorage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.True(operationalStateCM.CurrentActivityResponse.Type == ActivityResponse.RequestTerminate.ToString(), "Monitor_For_New_Media_Posted_v1 doesn't terminate container execution on null event payload");
        }

        [Test]
        public async Task Run_ShouldTerminateExecutionWhenChangedFieldIsNotMedia()
        {
            var activity = New<Monitor_For_New_Media_Posted_v1>();
            var activityContext = await ConfigureMonitorForNewMediaPosted(activity);

            var containerExecutionContext = FixtureData.FalseInstagramContainerExecutionContext();

            containerExecutionContext = await RunMonitorForNewMediaPosted(activity, activityContext, containerExecutionContext);
            var operationalStateCM = containerExecutionContext.PayloadStorage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.True(operationalStateCM.CurrentActivityResponse.Type == ActivityResponse.RequestTerminate.ToString(), "Monitor_For_New_Media_Posted_v1 doesn't terminate container execution when changed field doesn't contain media");
        }

        [Test]
        public async Task Run_ShouldTerminateExecutionWhenItCantFindPost()
        {
            var instagramIntegrationService = new Mock<IInstagramIntegration>();
            instagramIntegrationService.Setup(x => x.GetPostById(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);
            ObjectFactory.Container.Inject(typeof(IInstagramIntegration), instagramIntegrationService.Object);

            var activity = New<Monitor_For_New_Media_Posted_v1>();
            var activityContext = await ConfigureMonitorForNewMediaPosted(activity);
            var containerExecutionContext = FixtureData.FalseInstagramContainerExecutionContext();

            containerExecutionContext = await RunMonitorForNewMediaPosted(activity, activityContext, containerExecutionContext);
            var operationalStateCM = containerExecutionContext.PayloadStorage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.True(operationalStateCM.CurrentActivityResponse.Type == ActivityResponse.RequestTerminate.ToString(), "Monitor_For_New_Media_Posted_v1 doesn't terminate container execution when activity can't find related post on instagram");
        }

        [Test]
        public async Task Run_ShouldUpdateContainerPayloadWhenSucceeds()
        {
            var instagramIntegrationService = new Mock<IInstagramIntegration>();
            instagramIntegrationService.Setup(x => x.GetPostById(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new InstagramPost());
            ObjectFactory.Container.Inject(typeof(IInstagramIntegration), instagramIntegrationService.Object);

            var activity = New<Monitor_For_New_Media_Posted_v1>();
            var activityContext = await ConfigureMonitorForNewMediaPosted(activity);
            
            var containerExecutionContext = FixtureData.InstagramContainerExecutionContext();

            containerExecutionContext = await RunMonitorForNewMediaPosted(activity, activityContext, containerExecutionContext);
            var operationalStateCM = containerExecutionContext.PayloadStorage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.True(operationalStateCM.CurrentActivityResponse.Type == ActivityResponse.Success.ToString(), "Monitor_For_New_Media_Posted_v1 doesn't return success when everything is ok");

            instagramIntegrationService.Verify(x => x.GetPostById(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

    }
}
 