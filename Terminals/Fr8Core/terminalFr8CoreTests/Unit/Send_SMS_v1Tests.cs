using NUnit.Framework;
using terminalFr8Core.Actions;
using Fr8.Testing.Unit;
using System.Threading.Tasks;
using Moq;
using StructureMap;
using terminalUtilities.Twilio;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using terminalFr8Core.Activities;
using System.Configuration;

namespace terminalTests.Unit
{
    [TestFixture, Category("terminalFr8.Unit")]
    public class Send_SMS_v1Tests : BaseTest
    {
        private Send_SMS_v1 _sendSmsActivity;

        public override void SetUp()
        {
            base.SetUp();
            
            var fileds = new FieldDescriptionsCM(new FieldDTO[] { });

            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            //hubCommunicatorMock.Setup(h => h.GetPayload(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(payload));
           /* hubCommunicatorMock.Setup(h => h.GetDesignTimeFieldsByDirection(It.IsAny<Guid>(), It.IsAny<CrateDirection>(), 
                                        It.IsAny<AvailabilityType>())).Returns(Task.FromResult(fileds));*/
            ObjectFactory.Container.Inject(hubCommunicatorMock);
            ObjectFactory.Container.Inject(hubCommunicatorMock.Object);

            var twilioService = new Mock<ITwilioService>();
            twilioService.Setup(c => c.GetRegisteredSenderNumbers()).Returns(new List<string> { ConfigurationManager.AppSettings["TestPhoneNumber"] });
            twilioService.Setup(t => t.SendSms(It.IsAny<string>(), It.IsAny<string>())).Returns(new Twilio.Message());
            ObjectFactory.Container.Inject(twilioService.Object);
            ObjectFactory.Container.Inject(twilioService);
        }

        [Test]
        public async Task Initialize__CheckConfigControls()
        {
            //Arrage
            _sendSmsActivity = New<Send_SMS_v1>();

            //Act
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()

                }
            };
            await _sendSmsActivity.Configure(activityContext);

            //Assert
            var configControls = activityContext.ActivityPayload.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single();
            Assert.IsNotNull(configControls, "Send SMS is not initialized properly.");
            Assert.IsTrue(configControls.Content.Controls.Count == 2, "Send SMS configuration controls are not created correctly.");
        }

      /*  [Test]
        public async Task Initialize__CheckAvailableFields()
        {
            //Arrage
            _sendSmsActivity = New<Send_SMS_v1>();

            //Act
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()

                }
            };
            await _sendSmsActivity.Configure(activityContext);

            //Assert
            var availableFields = activityContext.ActivityPayload.CrateStorage
                                              .CratesOfType<FieldDescriptionsCM>()
                                              .Single(f => f.Label.Equals("Upstream Terminal-Provided Fields"));
            Assert.IsNotNull(availableFields, "Send SMS does not have available fields.");
        }*/

        [Test]
        public async Task Run_CheckSendCalledOnlyOnce()
        {
            //Arrage
            _sendSmsActivity = New<Send_SMS_v1>();
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

            var containerExecutionContext = new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
            };
            await _sendSmsActivity.Configure(activityContext);

            var controls = activityContext.ActivityPayload.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single();

            var smsNumber = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("SmsNumber"));
            smsNumber.TextValue = "+918055967968"; //This is some dummy number. This number is not valid

            var smsBody = controls.Content.Controls.OfType<TextSource>().Single(t => t.Name.Equals("SmsBody"));
            smsBody.TextValue = "some body";

            smsBody.ValueSource = smsNumber.ValueSource = "specific";

            //Act
            await _sendSmsActivity.Run(activityContext, containerExecutionContext);

            //Assert
            var messageDataCrate = containerExecutionContext.PayloadStorage.CratesOfType<StandardPayloadDataCM>().Single(p => p.Label.Equals("Message Data"));
            Assert.IsNotNull(messageDataCrate, "Send SMS activity is not run.");
            Assert.AreEqual(4, messageDataCrate.Content.AllValues().ToList().Count, "Message fields are not getting populated when Send SMS is executed.");

            var twilioService = Mock.Get(ObjectFactory.GetInstance<ITwilioService>());
            twilioService.Verify(t => t.SendSms(It.IsAny<string>(), It.IsAny<string>()));
        }
    }
}
