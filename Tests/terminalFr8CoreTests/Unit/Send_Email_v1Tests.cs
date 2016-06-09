using NUnit.Framework;
using Fr8.Testing.Unit;
using System.Threading.Tasks;
using Moq;
using StructureMap;
using System;
using System.Linq;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using terminalFr8Core.Activities;
using terminalUtilities.Interfaces;
using terminalUtilities.Models;

namespace terminalTests.Unit
{
    [TestFixture, Category("terminalFr8.Unit")]
    public class Send_Email_v1Tests : BaseTest
    {
        private Send_Email_v1 _sendEmailActivity;
        private ICrateManager _crate;
        private ActivityPayload activityPayload;

        public override void SetUp()
        {
            base.SetUp();

            var payload = new PayloadDTO(Guid.Empty);
            using (var storage = CrateManager.GetUpdatableStorage(payload))
            {
                storage.Add(Crate.FromContent(string.Empty, new OperationalStateCM()));
            }

            var userDTO = new UserDTO { FirstName = "First Name", LastName = "Last Name" };

            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            hubCommunicatorMock.Setup(h => h.GetPayload(It.IsAny<Guid>())).Returns(Task.FromResult(payload));
            hubCommunicatorMock.Setup(h => h.GetCurrentUser()).Returns(Task.FromResult(userDTO));
            ObjectFactory.Container.Inject(hubCommunicatorMock);
            ObjectFactory.Container.Inject(hubCommunicatorMock.Object);

            var emailPackagerMock = new Mock<IEmailPackager>();
            ObjectFactory.Container.Inject(emailPackagerMock.Object);
            ObjectFactory.Container.Inject(emailPackagerMock);
        }

        [Test]
        public async Task Initialize_CheckConfigControls()
        {
            //Arrage
            _sendEmailActivity = New<Send_Email_v1>();

            //Act
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                }
            };
            await _sendEmailActivity.Configure(activityContext);

            //Assert
            var configControls = activityContext.ActivityPayload.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single();
            Assert.IsNotNull(configControls, "Send Email is not initialized properly.");
            Assert.IsTrue(configControls.Content.Controls.Count == 3, "Send Email configuration controls are not created correctly.");
        }

        [Test]
        public async Task FollowUp_WithControlValues_CheckControlsValuesRetained()
        {
            //Arrage
            _sendEmailActivity = New<Send_Email_v1>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                }
            };
            await _sendEmailActivity.Configure(activityContext);

            var storage = activityContext.ActivityPayload.CrateStorage;
            var controls = storage.CratesOfType<StandardConfigurationControlsCM>().Single();
            var emailAddressControl = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("EmailAddress"));
            emailAddressControl.TextValue = "something@anything.com";

            //Act
            await _sendEmailActivity.Configure(activityContext);

            //Assert
            var configControls = storage.CratesOfType<StandardConfigurationControlsCM>().Single();
            var emailAddressCtl = configControls.Content.Controls.OfType<TextSource>().Single(c => c.Name.Equals("EmailAddress"));
            Assert.IsNotNull(emailAddressCtl, "Email Address field is missing in Send Email activity");
            Assert.AreEqual("something@anything.com", emailAddressCtl.TextValue, "Text value is not retained in Send Email activity configuration");
        }

        [Test]
        public async Task Run_CheckSendCalledOnlyOnce()
        {
            //Arrage
            _sendEmailActivity = New<Send_Email_v1>();
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
            var executionContext = new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
            };
            await _sendEmailActivity.Configure(activityContext);

            var storage = activityContext.ActivityPayload.CrateStorage;
            var controls = storage.CratesOfType<StandardConfigurationControlsCM>().Single();

            var emailAddressControl = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("EmailAddress"));
            emailAddressControl.TextValue = "something@anything.com";

            var subjectControl = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("EmailSubject"));
            subjectControl.TextValue = "some subject";

            var bodyControl = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("EmailBody"));
            bodyControl.TextValue = "some body";

            subjectControl.ValueSource = bodyControl.ValueSource = emailAddressControl.ValueSource = "specific";

            //Act
            await _sendEmailActivity.Run(activityContext,executionContext);

            //Assert
            var emailPackagerMock = Mock.Get(ObjectFactory.GetInstance<IEmailPackager>());
            emailPackagerMock.Verify(p => p.Send(It.IsAny<TerminalMailerDO>()), Times.Once());
        }
    }
}
