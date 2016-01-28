using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;

namespace terminalTwilioTests.Fixture
{
    class HealthMonitor_FixtureData
    {
        protected ICrateManager CrateManager;
        public HealthMonitor_FixtureData()
        {

            CrateManager = new CrateManager();
        }
        public static ActivityTemplateDTO Send_Via_Twilio_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Send_Via_Twilio_TEST",
                Version = "1"
            };
        }
        public static ActivityDTO Send_Via_Twilio_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Send_Via_Twilio_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Send_Via_Twilio",
                Label = "Send Via Twilio",
                AuthToken = null,
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }
        public ActivityDTO Send_Via_Twilio_v1_Preconfigured_Crate_With_No_SMS_Number()
        {
            var curActionDTO = Send_Via_Twilio_v1_InitialConfiguration_ActionDTO();
            using (var updater = CrateManager.UpdateStorage(curActionDTO))
            {
                var curCrate = No_SMS_Number_Controls();
                updater.CrateStorage.Add(curCrate);
            }
            return curActionDTO;
        }
        public Crate No_SMS_Number_Controls()
        {
            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                new TextSource()
                {
                    UpstreamSourceLabel = "Upstream Terminal-Provided Fields",
                    InitialLabel = "SMS Number",
                    Name = "SMS_Number",
                    Value = "15005550006",
                    Label = "SMS Number",
                },
                new TextBox()
                {
                    Label = "SMS Body",
                    Name = "SMS_Body",
                    Required = true,
                    Value = "That is the message that we are sending"
                }
            };
            return CrateManager.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }

        public TextSource GetUpstreamCrate()
        {
            return new TextSource
            {
                UpstreamSourceLabel = "Upsteam Crate",
                InitialLabel = "SMS Number",
                ValueSource = "specific",
                Name = "SMS_Number",
                Value = "+15005550006",
                Label = "SMS Number",
                Source = new FieldSourceDTO()
                {
                    Label = "SMS Number",
                    
                },
                Selected = true
            };
        }
    }
}
