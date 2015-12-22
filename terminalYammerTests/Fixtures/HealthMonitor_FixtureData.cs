using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;

namespace terminalYammerTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {

        public static ActivityTemplateDTO Post_To_Yammer_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Post_To_Yammer_TEST",
                Version = "1"
            };
        }

        public static AuthorizationTokenDTO Yammer_AuthToken()
        {
            return new AuthorizationTokenDTO()
            {
                Token = @"2166748-uX23UOpC7w8Y20rP5wwcLQ"
            };
        }


        internal static IEnumerable<FieldDTO> YammerEventFields()
        {
            return new List<FieldDTO>()
           {
              new FieldDTO("token", "sU3N7wdnhXmml1zR2dVLf6PV"),
               new FieldDTO("team_id", "T07F83QLE"),
               new FieldDTO("team_domain", "dockyardteam"),
               new FieldDTO("service_id", "16193135954"),
               new FieldDTO("channel_id", "C0BU4CH25"),
               new FieldDTO("channel_name", "Yammer - plugin - test"),
               new FieldDTO("timestamp", "1449594901.000014"),
               new FieldDTO("user_id"," U0BNK9P1N"),
               new FieldDTO("user_name", "sergeyp"),
               new FieldDTO("text", "test")
           };
        }

        public static ActionDTO Post_To_Yammer_v1_InitialConfiguration_ActionDTO(bool isAuthToken = true)
        {
            var activityTemplate = Post_To_Yammer_v1_ActivityTemplate();

            return new ActionDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Select Yammer Channel",
                Label = "Selected_Yammer_Channel",
                AuthToken = isAuthToken ? Yammer_AuthToken() : null,
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }
    }
}
