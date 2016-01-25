using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Hub.StructureMap;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalTwilio.Actions;
using terminalTwilio.Services;
using terminalTwilio.Tests.Fixtures;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using System;

namespace terminalTwilio.Tests.Actions
{
    [TestFixture]
    public class Send_Via_Twilio_v1Tests : BaseTest
    {
        private Send_Via_Twilio_v1 _twilioAction;
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

            var activityDO = FixtureData.ConfigureTwilioAction();
            var actionService = new Mock<IActivity>();
            actionService
                .Setup(c => c.MapFromDTO(It.IsAny<ActivityDTO>()))
                .Returns(activityDO);
            ObjectFactory.Configure(cfg => cfg.For<IActivity>().Use(actionService.Object));
            var activity = FixtureData.ConfigureTwilioAction();
            var baseTerminalAction = new Mock<BaseTerminalActivity>();
            baseTerminalAction
                .Setup(c => c.GetDesignTimeFields(It.IsAny<Guid>(), CrateDirection.Upstream, AvailabilityType.NotSet))
                .Returns(Task.FromResult(FixtureData.TestFields()));
            ObjectFactory.Configure(cfg => cfg.For<BaseTerminalActivity>().Use(baseTerminalAction.Object));

        }

        [Test]
        public void Configure_ReturnsCrateDTO()
        {
            _twilioAction = new Send_Via_Twilio_v1();
            var curActivityDO = FixtureData.ConfigureTwilioAction(); ;
            AuthorizationTokenDO curAuthTokenDO = FixtureData.AuthTokenDOTest1();
            var actionResult = _twilioAction.Configure(curActivityDO, curAuthTokenDO).Result;
            var controlsCrate = _crate.GetStorage(actionResult.CrateStorage).FirstOrDefault();
            Assert.IsNotNull(controlsCrate);

        }

        [Test]
        public void Configure_ReturnsCrateDTOStandardConfigurationControlsMS()
        {
            _twilioAction = new Send_Via_Twilio_v1();
            var curActivityDO = FixtureData.ConfigureTwilioAction();
            // ActionDTO curActionDTO = Mapper.Map<ActionDTO>(action);
            var curAuthTokenD0 = FixtureData.AuthTokenDOTest1();
            var actionResult = _twilioAction.Configure(curActivityDO, curAuthTokenD0).Result;

            var controlsCrate = _crate.GetStorage(actionResult.CrateStorage).CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            Assert.IsNotNull(controlsCrate);
        }

        [Test]
        public void Configure_ReturnsSMSAndSMSBodyFields()
        {
            _twilioAction = new Send_Via_Twilio_v1();
            var curActivityDO = FixtureData.ConfigureTwilioAction();
            //ActionDTO curActionDTO = Mapper.Map<ActionDTO>(action);

            var actionResult = _twilioAction.Configure(curActivityDO, null).Result;

            var standardControls = _crate.GetStorage(actionResult.CrateStorage).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            var smsNumberTextField = standardControls.Controls[0].Name;
            var smsNumberUpstreamField = standardControls.Controls[1].Name;
            var smsBodyFields = standardControls.FindByName("SMS_Body");

            Assert.AreEqual(smsNumberTextField, "SMS_Number");
            Assert.AreEqual(smsNumberUpstreamField, "SMS_Body");
            Assert.IsNotNull(smsBodyFields);
        }

        [Test]
        public void ParseSMSNumberAndMsg_ReturnsSMSNumberAndBody()
        {
            _twilioAction = new Send_Via_Twilio_v1();
            var crateDTO = FixtureData.CrateDTOForTwilioConfiguration();

            var smsINfo = _twilioAction.ParseSMSNumberAndMsg(crateDTO, null);

            Assert.AreEqual("+15005550006", smsINfo.Key);
            Assert.AreEqual("DO-1437 test", smsINfo.Value);
        }
    }
}
