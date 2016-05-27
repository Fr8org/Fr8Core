using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Infrastructure.AutoMapper;
using Hub.Interfaces;
using Fr8Data.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalTwilio.Tests.Fixtures;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using System;
using Fr8Data.Manifests;
using Fr8Data.States;
using UtilitiesTesting;
using terminalTwilio;
using terminalTwilio.Activities;
using Fr8Infrastructure.StructureMap;
using TerminalBase.Models;
using Fr8Data.Crates;
using System.Linq;
using terminalUtilities.Twilio;

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
            ObjectFactory.Configure(cfg => cfg.For<ITwilioService>().Use(new TwilioService()));
            TerminalBootstrapper.ConfigureTest();

            _crate = ObjectFactory.GetInstance<ICrateManager>();

            var twilioService = new Mock<ITwilioService>();
            twilioService
                .Setup(c => c.GetRegisteredSenderNumbers())
                .Returns(new List<string> { "+15005550006" });
            ObjectFactory.Configure(cfg => cfg.For<ITwilioService>().Use(twilioService.Object));

            var activityDO = FixtureData.ConfigureTwilioActivity();
            var actionService = new Mock<IActivity>();
            ObjectFactory.Configure(cfg => cfg.For<IActivity>().Use(actionService.Object));
            /*
            var activity = FixtureData.ConfigureTwilioActivity();
            
            var baseTerminalAction = new Mock<BaseTerminalActivity>();
            baseTerminalAction
                .Setup(c => c.GetDesignTimeFields(CrateDirection.Upstream, AvailabilityType.NotSet))
                .Returns(Task.FromResult(FixtureData.TestFields()));
            ObjectFactory.Configure(cfg => cfg.For<BaseTerminalActivity>().Use(baseTerminalAction.Object));
            */
            var hubCommunicator = new Mock<IHubCommunicator>();
            hubCommunicator.Setup(hc => hc.GetDesignTimeFieldsByDirection(
                                                It.IsAny<Guid>(), 
                                                CrateDirection.Upstream, 
                                                It.IsAny<AvailabilityType>(), 
                                                It.IsAny<string>())).Returns(Task.FromResult(FixtureData.TestFields()));
            ObjectFactory.Configure(cfg => cfg.For<IHubCommunicator>().Use(hubCommunicator.Object));
        }

        [Test]
        public async void Configure_ReturnsCrateDTO()
        {
            _twilioActivity = new Send_Via_Twilio_v1();
            var curActivityContext = FixtureData.ConfigureTwilioActivity();
            await _twilioActivity.Configure(curActivityContext);
            var controlsCrate = curActivityContext.ActivityPayload.CrateStorage.FirstOrDefault();
            Assert.IsNotNull(controlsCrate);
        }

        [Test]
        public async void Configure_ReturnsCrateDTOStandardConfigurationControlsMS()
        {
            _twilioActivity = new Send_Via_Twilio_v1();
            var curActivityContext = FixtureData.ConfigureTwilioActivity();
            await _twilioActivity.Configure(curActivityContext);
            var controlsCrate = curActivityContext.ActivityPayload.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            Assert.IsNotNull(controlsCrate);
        }

        [Test]
        public async void Configure_ReturnsSMSAndSMSBodyFields()
        {
            _twilioActivity = new Send_Via_Twilio_v1();
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
            Assert.AreEqual("+15005550006", smsINfo.Key);
            Assert.AreEqual("Unit Test Message", smsINfo.Value);
        }*/
    }
}
