using AutoMapper;
using Core.StructureMap;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json;
using NUnit.Framework;
using pluginTwilio.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesTesting.Fixtures;
using pluginTwilio;
using StructureMap;
using pluginTwilio.Services;
using Core.Interfaces;

namespace pluginTests.PluginTwilioTests.Actions
{
    [TestFixture]
    public class Send_Via_Twilio_v1Tests : BaseTest
    {
        private Send_Via_Twilio_v1 _twilioAction;
        private ICrate _crate;

        public override void SetUp()
        {
            base.SetUp();
            const StructureMapBootStrapper.DependencyType dependencyType = StructureMapBootStrapper.DependencyType.TEST;

            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            StructureMapBootStrapper.ConfigureDependencies(dependencyType).ConfigureTwilioDependencies(dependencyType);
            ObjectFactory.Configure(cfg => cfg.For<ITwilioService>().Use(new TwilioService()));
            _crate = ObjectFactory.GetInstance<ICrate>();
        }

        [Test]
        public void Configure_ReturnsCrateDTO()
        {
            _twilioAction = new Send_Via_Twilio_v1();
            var action = FixtureData.ConfigureTwilioAction();
            ActionDTO curActionDTO = Mapper.Map<ActionDTO>(action);
            
            var actionResult =_twilioAction.Configure(curActionDTO).Result;

            var controlsCrate = actionResult.CrateStorage.CrateDTO.FirstOrDefault();

            Assert.IsNotNull(controlsCrate);
        }

        [Test]
        public void Configure_ReturnsCrateDTOStandardConfigurationControlsMS()
        {
            _twilioAction = new Send_Via_Twilio_v1();
            var action = FixtureData.ConfigureTwilioAction();
            ActionDTO curActionDTO = Mapper.Map<ActionDTO>(action);

            var actionResult = _twilioAction.Configure(curActionDTO).Result;

            var controlsCrate = actionResult.CrateStorage.CrateDTO.FirstOrDefault();
            var standardControls = _crate.GetStandardConfigurationControls(controlsCrate);

            Assert.IsNotNull(standardControls);
        }

        [Test]
        public void Configure_ReturnsSMSAndSMSBodyFields()
        {
            _twilioAction = new Send_Via_Twilio_v1();
            var action = FixtureData.ConfigureTwilioAction();
            ActionDTO curActionDTO = Mapper.Map<ActionDTO>(action);

            var actionResult = _twilioAction.Configure(curActionDTO).Result;

            var controlsCrate = actionResult.CrateStorage.CrateDTO.FirstOrDefault();
            var standardControls = _crate.GetStandardConfigurationControls(controlsCrate);


            var smsNumberTextField = ((RadioButtonGroupControlDefinitionDTO)standardControls.Controls[0]).Radios.SelectMany(c => c.Controls).Where(s => s.Name == "SMS_Number").Count();
            var smsNumberUpstreamField = ((RadioButtonGroupControlDefinitionDTO)standardControls.Controls[0]).Radios.SelectMany(c => c.Controls).Where(s => s.Name == "upstream_crate").Count();
            var smsBodyFields = standardControls.FindByName("SMS_Body");


            Assert.Greater(smsNumberTextField, 0);
            Assert.Greater(smsNumberUpstreamField, 0);
            Assert.IsNotNull(smsBodyFields);
        }
    }
}
