using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Interfaces;
using Hub.Managers;
using Hub.StructureMap;
using terminalTwilio;
using terminalTwilio.Actions;
using terminalTwilio.Services;
using terminalTwilio.Tests.Fixtures;
using terminalTwilio.Tests;
using Data.Entities;

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
            _crate = ObjectFactory.GetInstance<ICrateManager>();

            var twilioService = new Mock<ITwilioService>();
            twilioService
                .Setup(c => c.GetRegisteredSenderNumbers())
                .Returns(new List<string>() { "+15005550006" });
            ObjectFactory.Configure(cfg => cfg.For<ITwilioService>().Use(twilioService.Object));

            var actionDO = FixtureData.ConfigureTwilioAction();
            var actionService = new Mock<IAction>();
            actionService
                .Setup(c => c.MapFromDTO(It.IsAny<ActionDTO>()))
                .Returns(actionDO);
            ObjectFactory.Configure(cfg => cfg.For<IAction>().Use(actionService.Object));

            var action = FixtureData.ConfigureTwilioAction();
        }

        [Test]
        public void Configure_ReturnsCrateDTO()
        {
            _twilioAction = new Send_Via_Twilio_v1();
            var curActionDO = FixtureData.ConfigureTwilioAction();;
            AuthorizationTokenDO curAuthTokenDO = FixtureData.AuthTokenDOTest1();
            var actionResult = _twilioAction.Configure(curActionDO, curAuthTokenDO).Result;
            
            var controlsCrate = actionResult.CrateStorageDTO().CrateDTO[0];

            var controlsCrate =  _crate.FromDto(actionResult.CrateStorage).FirstOrDefault();

            Assert.IsNotNull(controlsCrate);
        }

        [Test]
        public void Configure_ReturnsCrateDTOStandardConfigurationControlsMS()
        {
            _twilioAction = new Send_Via_Twilio_v1();
            var curActionDO = FixtureData.ConfigureTwilioAction();
           // ActionDTO curActionDTO = Mapper.Map<ActionDTO>(action);
            var curAuthTokenD0 = FixtureData.AuthTokenDOTest1();
            var actionResult = _twilioAction.Configure(curActionDO, curAuthTokenD0).Result;

            var controlsCrate = _crate.FromDto(actionResult.CrateStorage).CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            Assert.IsNotNull(controlsCrate);
        }

        [Test]
        public void Configure_ReturnsSMSAndSMSBodyFields()
        {
            _twilioAction = new Send_Via_Twilio_v1();
            var curActionDO = FixtureData.ConfigureTwilioAction();
            //ActionDTO curActionDTO = Mapper.Map<ActionDTO>(action);

            var actionResult = _twilioAction.Configure(curActionDO, null).Result;

            var standardControls = _crate.FromDto(actionResult.CrateStorage).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            var smsNumberTextField = ((RadioButtonGroupControlDefinitionDTO)standardControls.Controls[0]).Radios.SelectMany(c => c.Controls).Where(s => s.Name == "SMS_Number").Count();
            var smsNumberUpstreamField = ((RadioButtonGroupControlDefinitionDTO)standardControls.Controls[0]).Radios.SelectMany(c => c.Controls).Where(s => s.Name == "upstream_crate").Count();
            var smsBodyFields = standardControls.FindByName("SMS_Body");


            Assert.Greater(smsNumberTextField, 0);
            Assert.Greater(smsNumberUpstreamField, 0);
            Assert.IsNotNull(smsBodyFields);
        }

        [Test]
        public void ParseSMSNumberAndMsg_ReturnsSMSNumberAndBody()
        {
            _twilioAction = new Send_Via_Twilio_v1();
            var crateDTO = FixtureData.CrateDTOForTwilioConfiguration();

            var smsINfo = _twilioAction.ParseSMSNumberAndMsg(crateDTO);

            Assert.AreEqual(smsINfo.Key, "+15005550006");
            Assert.AreEqual(smsINfo.Value, "DocuSign Sent");
        }
    }
}
