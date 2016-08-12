using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using System.Configuration;

namespace terminalTwilioTests.Fixture
{
    class HealthMonitor_FixtureData
    {
        protected ICrateManager CrateManager;
        public HealthMonitor_FixtureData()
        {

            CrateManager = new CrateManager();
        }
        public static ActivityTemplateSummaryDTO Send_Via_Twilio_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Send_Via_Twilio_TEST",
                Version = "1"
            };
        }
        public static Fr8DataDTO Send_Via_Twilio_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Send_Via_Twilio_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Send SMS",
                AuthToken = null,
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public ActivityDTO Send_Via_Twilio_v1_Preconfigured_Crate_With_No_SMS_Number()
        {
            var dataDTO = Send_Via_Twilio_v1_InitialConfiguration_Fr8DataDTO();
            using (var crateStorage = CrateManager.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                var curCrate = No_SMS_Number_Controls();
                crateStorage.Add(curCrate);
            }
            return dataDTO.ActivityDTO;
        }

        public Crate No_SMS_Number_Controls()
        {
            var controls = new StandardConfigurationControlsCM(
                new TextSource
                {
                    UpstreamSourceLabel = "Upstream Terminal-Provided Fields",
                    InitialLabel = "SMS Number",
                    Name = "SMS_Number",
                    Value = ConfigurationManager.AppSettings["TestPhoneNumber"],
                    Label = "SMS Number"
                },

                new TextSource
                {
                    UpstreamSourceLabel = "Upstream Terminal-Provided Fields",
                    InitialLabel = "SMS Body",
                    Name = "SMS_Body",
                    Value = "That is the message that we are sending",
                    Label = "SMS Body"
                });
            
            return Crate.FromContent("Configuration_Controls", controls);
        }

        public TextSource GetUpstreamCrate()
        {
            return new TextSource
            {
                UpstreamSourceLabel = "Upsteam Crate",
                InitialLabel = "SMS Number",
                ValueSource = "specific",
                Name = "SMS_Number",
                Value = ConfigurationManager.AppSettings["TestPhoneNumber"],
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
