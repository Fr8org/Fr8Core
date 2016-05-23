using Fr8Data.Crates;
using Fr8Data.Manifests;
using NUnit.Framework;
using terminalFr8Core.Actions;
using UtilitiesTesting;
using TerminalBase.Infrastructure;
using System.Threading.Tasks;
using Moq;
using StructureMap;
using Hub.Managers;
using System.Linq;
using Fr8Data.Control;
using Data.Interfaces;
using Data.Entities;
using System;
using Fr8Data.DataTransferObjects;
using Data.States;
using Fr8Data.States;
using terminalUtilities.Twilio;
using System.Collections.Generic;

namespace terminalTests.Unit
{
    [TestFixture, Category("terminalFr8.Unit")]
    public class Send_SMS_v1Tests : BaseTest
    {
        private Send_SMS_v1 _sendSmsActivity;

        public override void SetUp()
        {
            base.SetUp();

            var payload = new PayloadDTO(Guid.Empty);
            using (var storage = CrateManager.GetUpdatableStorage(payload))
            {
                storage.Add(Crate.FromContent(string.Empty, new OperationalStateCM()));
            }

            var fileds = new FieldDescriptionsCM(new FieldDTO[] { });

            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            hubCommunicatorMock.Setup(h => h.GetPayload(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(payload));
            hubCommunicatorMock.Setup(h => h.GetDesignTimeFieldsByDirection(It.IsAny<ActivityDO>(), It.IsAny<CrateDirection>(), 
                                        It.IsAny<AvailabilityType>(), It.IsAny<string>())).Returns(Task.FromResult(fileds));
            ObjectFactory.Container.Inject(hubCommunicatorMock);
            ObjectFactory.Container.Inject(hubCommunicatorMock.Object);

            var twilioService = new Mock<ITwilioService>();
            twilioService.Setup(c => c.GetRegisteredSenderNumbers()).Returns(new List<string> { "+15005550006" });
            twilioService.Setup(t => t.SendSms(It.IsAny<string>(), It.IsAny<string>())).Returns(new Twilio.Message());
            ObjectFactory.Container.Inject(twilioService.Object);
            ObjectFactory.Container.Inject(twilioService);
        }

        [Test]
        public async Task Initialize__CheckConfigControls()
        {
            //Arrage
            _sendSmsActivity = new Send_SMS_v1();

            //Act
            var curActivity = await _sendSmsActivity.Configure(new ActivityDO(), null);

            //Assert
            var configControls = CrateManager.GetStorage(curActivity).CratesOfType<StandardConfigurationControlsCM>().Single();
            Assert.IsNotNull(configControls, "Send SMS is not initialized properly.");
            Assert.IsTrue(configControls.Content.Controls.Count == 2, "Send SMS configuration controls are not created correctly.");
        }

        [Test]
        public async Task Initialize__CheckAvailableFields()
        {
            //Arrage
            _sendSmsActivity = new Send_SMS_v1();

            //Act
            var curActivity = await _sendSmsActivity.Configure(new ActivityDO(), null);

            //Assert
            var availableFields = CrateManager.GetStorage(curActivity)
                                              .CratesOfType<FieldDescriptionsCM>()
                                              .Single(f => f.Label.Equals("Upstream Terminal-Provided Fields"));
            Assert.IsNotNull(availableFields, "Send SMS does not have available fields.");
        }

        [Test]
        public async Task Run_CheckSendCalledOnlyOnce()
        {
            //Arrage
            _sendSmsActivity = new Send_SMS_v1();
            var curActivity = await _sendSmsActivity.Configure(new Data.Entities.ActivityDO(), null);

            using (var storage = CrateManager.GetUpdatableStorage(curActivity))
            {
                var controls = storage.CratesOfType<StandardConfigurationControlsCM>().Single();

                var smsNumber = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("SmsNumber"));
                smsNumber.TextValue = "+918055967968"; //This is some dummy number. This number is not valid

                var smsBody = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("SmsBody"));
                smsBody.TextValue = "some body";

                smsBody.ValueSource = smsNumber.ValueSource = "specific";
            }

            //Act
            var payLoad = await _sendSmsActivity.Run(curActivity, new System.Guid(), new AuthorizationTokenDO { Token = "1" });

            //Assert
            var messageDataCrate = CrateManager.GetStorage(payLoad).CratesOfType<StandardPayloadDataCM>().Single(p => p.Label.Equals("Message Data"));
            Assert.IsNotNull(messageDataCrate, "Send SMS activity is not run.");
            Assert.AreEqual(4, messageDataCrate.Content.AllValues().ToList().Count, "Message fields are not getting populated when Send SMS is executed.");

            var twilioService = Mock.Get(ObjectFactory.GetInstance<ITwilioService>());
            twilioService.Verify(t => t.SendSms(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }
    }
}
