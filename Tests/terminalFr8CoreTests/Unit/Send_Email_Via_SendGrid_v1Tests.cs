using System.Collections.Generic;
using Fr8Data.Crates;
using Fr8Data.Manifests;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using terminalFr8Core;
using terminalFr8Core.Actions;
using UtilitiesTesting;
using TerminalBase.Infrastructure;
using System.Threading.Tasks;
using Moq;
using StructureMap;
using terminalUtilities.SendGrid;
using Hub.Managers;
using System.Linq;
using Fr8Data.Control;
using Data.Interfaces;
using Data.Entities;
using System;
using Fr8Data.DataTransferObjects;

namespace terminalTests.Unit
{
    [TestFixture, Category("terminalFr8.Unit")]
    public class Send_Email_Via_SendGrid_v1Tests : BaseTest
    {
        private Send_Email_Via_SendGrid_v1 _sendEmailActivity;

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
            hubCommunicatorMock.Setup(h => h.GetPayload(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(payload));
            hubCommunicatorMock.Setup(h => h.GetCurrentUser(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(userDTO));
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
            _sendEmailActivity = new Send_Email_Via_SendGrid_v1();

            //Act
            var curActivity = await _sendEmailActivity.Configure(new Data.Entities.ActivityDO(), null);

            //Assert
            var configControls = CrateManager.GetStorage(curActivity).CratesOfType<StandardConfigurationControlsCM>().Single();
            Assert.IsNotNull(configControls, "Send Email is not initialized properly.");
            Assert.IsTrue(configControls.Content.Controls.Count == 3, "Send Email configuration controls are not created correctly.");
        }

        [Test]
        public async Task FollowUp_WithControlValues_CheckControlsValuesRetained()
        {
            //Arrage
            _sendEmailActivity = new Send_Email_Via_SendGrid_v1();
            var curActivity = await _sendEmailActivity.Configure(new Data.Entities.ActivityDO(), null);

            using (var storage = CrateManager.GetUpdatableStorage(curActivity))
            {
                var controls = storage.CratesOfType<StandardConfigurationControlsCM>().Single();
                var emailAddressControl = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("EmailAddress"));
                emailAddressControl.TextValue = "something@anything.com";
            }

            //Act
            curActivity = await _sendEmailActivity.Configure(curActivity, null);

            //Assert
            var configControls = CrateManager.GetStorage(curActivity).CratesOfType<StandardConfigurationControlsCM>().Single();
            var emailAddressCtl = configControls.Content.Controls.OfType<TextSource>().Single(c => c.Name.Equals("EmailAddress"));
            Assert.IsNotNull(emailAddressCtl, "Email Address field is missing in Send Email activity");
            Assert.AreEqual("something@anything.com", emailAddressCtl.TextValue, "Text value is not retained in Send Email activity configuration");
        }

        [Test]
        public async Task Run_CheckSendCalledOnlyOnce()
        {
            //Arrage
            _sendEmailActivity = new Send_Email_Via_SendGrid_v1();
            var curActivity = await _sendEmailActivity.Configure(new Data.Entities.ActivityDO(), null);

            using (var storage = CrateManager.GetUpdatableStorage(curActivity))
            {
                var controls = storage.CratesOfType<StandardConfigurationControlsCM>().Single();

                var emailAddressControl = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("EmailAddress"));
                emailAddressControl.TextValue = "something@anything.com";

                var subjectControl = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("EmailSubject"));
                subjectControl.TextValue = "some subject";

                var bodyControl = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("EmailBody"));
                bodyControl.TextValue = "some body";

                subjectControl.ValueSource = bodyControl.ValueSource = emailAddressControl.ValueSource = "specific";
            }

            //Act
            var payLoad = await _sendEmailActivity.Run(curActivity, new System.Guid(), new AuthorizationTokenDO { Token = "1" });

            //Assert
            var emailPackagerMock = Mock.Get(ObjectFactory.GetInstance<IEmailPackager>());
            emailPackagerMock.Verify(p => p.Send(It.IsAny<IMailerDO>()), Times.Once());
        }
    }
}
