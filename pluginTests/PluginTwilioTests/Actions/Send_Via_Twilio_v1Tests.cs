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

namespace pluginTests.PluginTwilioTests.Actions
{
    [TestFixture]
    public class Send_Via_Twilio_v1Tests : BaseTest
    {
        private Send_Via_Twilio_v1 _twilioAction;

        public override void SetUp()
        {
            base.SetUp();
            const StructureMapBootStrapper.DependencyType dependencyType = StructureMapBootStrapper.DependencyType.TEST;

            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            StructureMapBootStrapper.ConfigureDependencies(dependencyType).ConfigureTwilioDependencies(dependencyType);
            ObjectFactory.Configure(cfg => cfg.For<ITwilioService>().Use(new TwilioService()));
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
            var standardControls = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(controlsCrate.Contents);

            Assert.IsNotNull(standardControls);

            var smsNumberFields = standardControls.FindByName("SMS_Number");
            var smsBodyFields = standardControls.FindByName("SMS_Body");
        }

        [Test]
        public void Configure_ReturnsSMSAndSMSBodyFields()
        {
            _twilioAction = new Send_Via_Twilio_v1();
            var action = FixtureData.ConfigureTwilioAction();
            ActionDTO curActionDTO = Mapper.Map<ActionDTO>(action);

            var actionResult = _twilioAction.Configure(curActionDTO).Result;

            var controlsCrate = actionResult.CrateStorage.CrateDTO.FirstOrDefault();
            var standardControls = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(controlsCrate.Contents);


            var smsNumberFields = standardControls.FindByName("SMS_Number");
            var smsBodyFields = standardControls.FindByName("SMS_Body");


            Assert.IsNotNull(smsNumberFields);
            Assert.IsNotNull(smsBodyFields);
        }
    }
}
