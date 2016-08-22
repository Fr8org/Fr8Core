using System.Collections.Generic;
using Data.Infrastructure.AutoMapper;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalTwilio.Tests.Fixtures;
using Fr8.Testing.Unit;
using terminalTwilio.Activities;
using System.Linq;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.StructureMap;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Interfaces;
using terminalTwilio;
using terminalUtilities.Twilio;
using IActivity = Hub.Interfaces.IActivity;
using System.Configuration;

namespace terminalTwilioTests.Activities
{
    [TestFixture]
    public class Send_Via_Twilio_v1Tests : BaseTest
    {
        private Send_Via_Twilio_v1 _twilioActivity;
        private ICrateManager _crate;

        public override void SetUp()
        {
            base.SetUp();
            const StructureMapBootStrapper.DependencyType dependencyType = StructureMapBootStrapper.DependencyType.TEST;

            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            StructureMapBootStrapper.ConfigureDependencies(dependencyType).ConfigureTwilioDependencies(dependencyType);
            TerminalBootstrapper.ConfigureTest();

            _crate = ObjectFactory.GetInstance<ICrateManager>();

            var twilioService = new Mock<ITwilioService>();
            twilioService
                .Setup(c => c.GetRegisteredSenderNumbers())
                .Returns(new List<string> { ConfigurationManager.AppSettings["TestPhoneNumber"] });
            ObjectFactory.Configure(cfg => cfg.For<ITwilioService>().Use(twilioService.Object));

            var activityDO = FixtureData.ConfigureTwilioActivity();
            var actionService = new Mock<IActivity>();
            ObjectFactory.Configure(cfg => cfg.For<IActivity>().Use(actionService.Object));
            /*
            var activity = FixtureData.ConfigureTwilioActivity();
            
            var baseTerminalAction = new Mock<ExplicitTerminalActivity>();
            baseTerminalAction
                .Setup(c => c.GetDesignTimeFields(CrateDirection.Upstream, AvailabilityType.NotSet))
                .Returns(Task.FromResult(FixtureData.TestFields()));
            ObjectFactory.Configure(cfg => cfg.For<ExplicitTerminalActivity>().Use(baseTerminalAction.Object));
            */
            var hubCommunicator = new Mock<IHubCommunicator>();
           /* hubCommunicator.Setup(hc => hc.GetDesignTimeFieldsByDirection(
                                                It.IsAny<Guid>(), 
                                                CrateDirection.Upstream, 
                                                It.IsAny<AvailabilityType>())).Returns(Task.FromResult(FixtureData.TestFields()));*/
            ObjectFactory.Configure(cfg => cfg.For<IHubCommunicator>().Use(hubCommunicator.Object));
        }

        [Test]
        public async void Configure_ReturnsCrateDTO()
        {
            _twilioActivity = New<Send_Via_Twilio_v1>();
            var curActivityContext = FixtureData.ConfigureTwilioActivity();
            await _twilioActivity.Configure(curActivityContext);
            var controlsCrate = curActivityContext.ActivityPayload.CrateStorage.FirstOrDefault();
            Assert.IsNotNull(controlsCrate);
        }

        [Test]
        public async void Configure_ReturnsCrateDTOStandardConfigurationControlsMS()
        {
            _twilioActivity = New<Send_Via_Twilio_v1>();
            var curActivityContext = FixtureData.ConfigureTwilioActivity();
            await _twilioActivity.Configure(curActivityContext);
            var controlsCrate = curActivityContext.ActivityPayload.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            Assert.IsNotNull(controlsCrate);
        }

        [Test]
        public async void Configure_ReturnsSMSAndSMSBodyFields()
        {
            _twilioActivity = New<Send_Via_Twilio_v1>();
            var curActivityContext = FixtureData.ConfigureTwilioActivity();
            //ActionDTO curActionDTO = Mapper.Map<ActionDTO>(action);
            await _twilioActivity.Configure(curActivityContext);
            var standardControls = curActivityContext.ActivityPayload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            var smsNumberTextField = standardControls.Controls[0].Name;
            var smsNumberUpstreamField = standardControls.Controls[1].Name;
            var smsBodyFields = standardControls.FindByName("SMS_Body");

            Assert.AreEqual(smsNumberTextField, "SMS_Number");
            Assert.AreEqual(smsNumberUpstreamField, "SMS_Body");
            Assert.IsNotNull(smsBodyFields);
        }

        //TODO run send twilio activity and use moq to verify sms and body are extracted
        //correctly
        /*
        [Test]
        public void ParseSMSNumberAndMsg_ReturnsSMSNumberAndBody()
        {
            _twilioActivity = new Send_Via_Twilio_v1();
            var crateDTO = FixtureData.CrateDTOForTwilioConfiguration();
           var smsINfo = _twilioActivity.ParseSMSNumberAndMsg();
            Assert.AreEqual(ConfigurationManager.AppSettings["TestPhoneNumber"], smsINfo.Key);
            Assert.AreEqual("Unit Test Message", smsINfo.Value);
        }*/
    }
}
