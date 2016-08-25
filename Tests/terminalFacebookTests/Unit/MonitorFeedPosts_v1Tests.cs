using Moq;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using terminalFacebook.Activities;
using Fr8.Testing.Unit;
using terminalFacebook.Interfaces;
using terminalFacebook.Models;

namespace terminalFacebookTests.Unit
{
    [TestFixture]
    public class MonitorFeedPosts_v1Tests : BaseTest
    {
        public override void SetUp()
        {
            base.SetUp();
            var hubMock = new Mock<IHubCommunicator>();
            var fbIntegrationService = new Mock<IFacebookIntegration>();
            ObjectFactory.Container.Inject(typeof(IFacebookIntegration), fbIntegrationService.Object);
            ObjectFactory.Container.Inject(hubMock);
            ObjectFactory.Container.Inject(hubMock.Object);
        }

        [Test]
        public async Task Initialize_CheckConfigControls()
        {
            var activity = New<Monitor_Feed_Posts_v1>();
            var activityContext = await ConfigureMonitorFeedPosts(activity);
            var configControls = activityContext.ActivityPayload.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            Assert.IsNotNull(configControls, "Monitor_Feed_Posts_v1 is not initialized properly.");
            Assert.IsTrue(configControls.Content.Controls.Count == 1, "Monitor_Feed_Posts_v1 configuration controls are not created correctly.");
            Assert.IsTrue(configControls.Content.Controls.Single().Type == ControlTypes.TextBlock, "Monitor_Feed_Posts_v1 configuration controls are not created correctly. It should contain a single textBlock");
        }


        [Test]
        public async Task Initialize_ShouldHaveCorrectCrateStructure()
        {
            var activity = New<Monitor_Feed_Posts_v1>();
            var activityContext = await ConfigureMonitorFeedPosts(activity);

            var crateDescriptions = activityContext.ActivityPayload.CrateStorage.CratesOfType<CrateDescriptionCM>().FirstOrDefault();
            Assert.IsNotNull(crateDescriptions, "Monitor_Feed_Posts_v1 has no crate descriptions in initial response");
            var descriptions = crateDescriptions.Content.CrateDescriptions;
            Assert.AreEqual(1, descriptions.Count, "Monitor_Feed_Posts_v1 should publish single description");
            var standardPayloadDescription = descriptions.Single();
            Assert.AreEqual("Monitor Facebook Runtime Fields", standardPayloadDescription.Label, "Monitor_Feed_Posts_v1 should publish crate with label Monitor Facebook Runtime Fields");
            Assert.AreEqual("Standard Payload Data", standardPayloadDescription.ManifestType, "Monitor_Feed_Posts_v1 should publish Standard Payload Data");
            Assert.AreEqual(4, standardPayloadDescription.Fields.Count, "Monitor_Feed_Posts_v1 should publish 4 fields");
            Assert.IsTrue(standardPayloadDescription.Fields.Any(f => f.Name == "Feed Id"), "Monitor_Feed_Posts_v1 should publish Feed Id field as runtime");
            Assert.IsTrue(standardPayloadDescription.Fields.Any(f => f.Name == "Feed Message"), "Monitor_Feed_Posts_v1 should publish Feed Message field as runtime");
            Assert.IsTrue(standardPayloadDescription.Fields.Any(f => f.Name == "Feed Story"), "Monitor_Feed_Posts_v1 should publish Feed Story field as runtime");
            Assert.IsTrue(standardPayloadDescription.Fields.Any(f => f.Name == "Feed Time"), "Monitor_Feed_Posts_v1 should publish Feed Time field as runtime");

            var eventSubscriptionCM = activityContext.ActivityPayload.CrateStorage.CratesOfType<EventSubscriptionCM>().FirstOrDefault();
            Assert.IsNotNull(eventSubscriptionCM, "Monitor_Feed_Posts_v1 has no event subscription crate in initial response");
            var subscription = eventSubscriptionCM.Content;
            Assert.AreEqual(1, subscription.Subscriptions.Count, "Monitor_Feed_Posts_v1 should subscribe to a single event");
            Assert.AreEqual("Standard Event Subscription", subscription.ManifestType.Type, "Monitor_Feed_Posts_v1 should publish Standard Event Subscriptions");
            Assert.AreEqual("Facebook", subscription.Manufacturer, "Monitor_Feed_Posts_v1 should subscribe to Facebook Manufacturer");
            Assert.IsTrue(subscription.Subscriptions.Any(f => f == "feed"), "Monitor_Feed_Posts_v1 should subscribe to feed event");
        }

        private async Task<ActivityContext> ConfigureMonitorFeedPosts(Monitor_Feed_Posts_v1 activity)
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

        private async Task<ContainerExecutionContext> RunMonitorFeedPosts(Monitor_Feed_Posts_v1 activity,
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
            var activity = New<Monitor_Feed_Posts_v1>();
            //Arrage
            var activityContext = await ConfigureMonitorFeedPosts(activity);
            //Act
            var containerExecutionContext = await RunMonitorFeedPosts(activity, activityContext);
            var operationalStateCM = containerExecutionContext.PayloadStorage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.True(operationalStateCM.CurrentActivityResponse.Type == ActivityResponse.RequestTerminate.ToString(), "Monitor_Feed_Posts_v1 doesn't terminate container execution on null event payload");
        }

        [Test]
        public async Task Run_ShouldTerminateExecutionWhenChangedFieldIsNotFeed()
        {
            var activity = New<Monitor_Feed_Posts_v1>();
            //Arrage
            var activityContext = await ConfigureMonitorFeedPosts(activity);
            var fbEventCM = new FacebookUserEventCM
            {
                Id = "123",
                ChangedFields = new string[] {"test"},
                Time = "123",
                UserId = "123"
            };
            var eventReportCrate = new EventReportCM
            {
                EventNames = "feed",
                EventPayload = new CrateStorage(Crate.FromContent("Facebook user event", fbEventCM))
            };
            var containerExecutionContext = new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()), Crate.FromContent("Facebook user event", eventReportCrate))
            };

            //Act
            containerExecutionContext = await RunMonitorFeedPosts(activity, activityContext, containerExecutionContext);
            var operationalStateCM = containerExecutionContext.PayloadStorage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.True(operationalStateCM.CurrentActivityResponse.Type == ActivityResponse.RequestTerminate.ToString(), "Monitor_Feed_Posts_v1 doesn't terminate container execution when changed field doesn't contain feed");
        }


        [Test]
        public async Task Run_ShouldTerminateExecutionWhenItCantFindPost()
        {
            var fbIntegrationService = new Mock<IFacebookIntegration>();
            fbIntegrationService.Setup(x => x.GetPostById(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);
            ObjectFactory.Container.Inject(typeof(IFacebookIntegration), fbIntegrationService.Object);

            var activity = New<Monitor_Feed_Posts_v1>();
            //Arrage
            var activityContext = await ConfigureMonitorFeedPosts(activity);
            var fbEventCM = new FacebookUserEventCM
            {
                Id = "123",
                ChangedFields = new string[] { "feed" },
                Time = "123",
                UserId = "123"
            };
            var eventReportCrate = new EventReportCM
            {
                EventNames = "feed",
                EventPayload = new CrateStorage(Crate.FromContent("Facebook user event", fbEventCM))
            };
            var containerExecutionContext = new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()), Crate.FromContent("Facebook user event", eventReportCrate))
            };

            //Act
            containerExecutionContext = await RunMonitorFeedPosts(activity, activityContext, containerExecutionContext);
            var operationalStateCM = containerExecutionContext.PayloadStorage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.True(operationalStateCM.CurrentActivityResponse.Type == ActivityResponse.RequestTerminate.ToString(), "Monitor_Feed_Posts_v1 doesn't terminate container execution when activity can't find related post on facebook");
        }


        [Test]
        public async Task Run_ShouldUpdateContainerPayloadWhenSucceeds()
        {
            var fbIntegrationService = new Mock<IFacebookIntegration>();
            fbIntegrationService.Setup(x => x.GetPostById(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new FacebookPost());
            ObjectFactory.Container.Inject(typeof(IFacebookIntegration), fbIntegrationService.Object);

            var activity = New<Monitor_Feed_Posts_v1>();
            //Arrage
            var activityContext = await ConfigureMonitorFeedPosts(activity);
            var fbEventCM = new FacebookUserEventCM
            {
                Id = "123",
                ChangedFields = new string[] { "feed" },
                Time = "123",
                UserId = "123"
            };
            var eventReportCrate = new EventReportCM
            {
                EventNames = "feed",
                EventPayload = new CrateStorage(Crate.FromContent("Facebook user event", fbEventCM))
            };
            var containerExecutionContext = new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()), Crate.FromContent("Facebook user event", eventReportCrate))
            };

            //Act
            containerExecutionContext = await RunMonitorFeedPosts(activity, activityContext, containerExecutionContext);
            var operationalStateCM = containerExecutionContext.PayloadStorage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.True(operationalStateCM.CurrentActivityResponse.Type == ActivityResponse.Success.ToString(), "Monitor_Feed_Posts_v1 doesn't return success when everything is ok");

            fbIntegrationService.Verify(x => x.GetPostById(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }


    }
}
