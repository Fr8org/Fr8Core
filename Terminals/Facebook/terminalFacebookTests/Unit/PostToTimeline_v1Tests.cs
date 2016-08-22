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
    public class PostToTimeline_v1Tests : BaseTest
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

        private async Task<ActivityContext> ConfigurePostToTimeline(Post_To_Timeline_v1 activity)
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

        private async Task<ContainerExecutionContext> RunPostToTimeline(Post_To_Timeline_v1 activity,
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
        public async Task Initialize_CheckConfigControls()
        {
            var activity = New<Post_To_Timeline_v1>();
            var activityContext = await ConfigurePostToTimeline(activity);
            var configControls = activityContext.ActivityPayload.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            Assert.IsNotNull(configControls, "Post_To_Timeline_v1 is not initialized properly.");
            Assert.IsTrue(configControls.Content.Controls.Count == 1, "Post_To_Timeline_v1 configuration controls are not created correctly.");
            Assert.IsTrue(configControls.Content.Controls.Single().Type == ControlTypes.TextSource, "Post_To_Timeline_v1 configuration controls are not created correctly. It should contain a single TextSource");
        }

        [Test]
        public async Task Run_ShouldPostToFacebook()
        {
            var fbIntegrationService = new Mock<IFacebookIntegration>();
            fbIntegrationService.Setup(x => x.PostToTimeline(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(0));
            ObjectFactory.Container.Inject(typeof(IFacebookIntegration), fbIntegrationService.Object);

            var activity = New<Post_To_Timeline_v1>();
            //Arrage
            var activityContext = await ConfigurePostToTimeline(activity);
            var containerExecutionContext = new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
            };
            var controls = activityContext.ActivityPayload.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single();
            var smsNumber = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("Message"));
            smsNumber.TextValue = "Test";
            //Act
            containerExecutionContext = await RunPostToTimeline(activity, activityContext, containerExecutionContext);
            var operationalStateCM = containerExecutionContext.PayloadStorage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.True(operationalStateCM.CurrentActivityResponse.Type == ActivityResponse.Success.ToString(), "Post_To_Timeline_v1 doesn't return success when everything is ok");
            fbIntegrationService.Verify(x => x.PostToTimeline(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
