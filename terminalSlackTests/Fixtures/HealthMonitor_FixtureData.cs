using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;

namespace terminalSlackTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static ActivityDTO Monitor_Channel_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Monitor_Channel_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Monitor_Channel DocuSign",
                AuthToken = Slack_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }

        public static ActivityTemplateDTO Monitor_Channel_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Monitor_Channel_TEST",
                Version = "1"
            };
        }

        public static AuthorizationTokenDTO Slack_AuthToken()
        {
            return new AuthorizationTokenDTO()
            {
                Token = @"xoxp-9815816992-9816213634-14997343526-d99a1c9198"
            };
        }


        internal static IEnumerable<FieldDTO> SlackEventFields()
        {
            return new List<FieldDTO>()
           {
              new FieldDTO("token", "sU3N7wdnhXmml1zR2dVLf6PV"),
               new FieldDTO("team_id", "T07F83QLE"),
               new FieldDTO("team_domain", "dockyardteam"),
               new FieldDTO("service_id", "16193135954"),
               new FieldDTO("channel_id", "C09Q069KL"),
               new FieldDTO("channel_name", "slack - plugin - test"),
               new FieldDTO("timestamp", "1449594901.000014"),
               new FieldDTO("user_id"," U0BNK9P1N"),
               new FieldDTO("user_name", "sergeyp"),
               new FieldDTO("text", "test")
           };
        }

        public static ActivityDTO Publish_To_Slack_v1_InitialConfiguration_ActionDTO(bool isAuthToken = true)
        {
            var activityTemplate = Monitor_Channel_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Selected_Slack_Channel",
                AuthToken = isAuthToken ? Slack_AuthToken() : null,
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }
    }
}
