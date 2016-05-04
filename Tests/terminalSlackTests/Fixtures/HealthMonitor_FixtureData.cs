using System;
using System.Collections.Generic;
using Fr8Data.DataTransferObjects;
using Fr8Data.States;

namespace terminalSlackTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static Fr8DataDTO Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Monitor_Channel_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO
            {
                Id = Guid.NewGuid(),
                Label = "Monitor_Channel DocuSign",
                AuthToken = Slack_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static ActivityTemplateDTO Monitor_Channel_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO
            {
                Id = Guid.NewGuid(),
                Name = "Monitor_Channel_TEST",
                Label = "Monitor Channel",
                Category = ActivityCategory.Monitors,
                Terminal = new TerminalDTO
                {
                    AuthenticationType = AuthenticationType.Internal
                },
                Version = "1"
            };
        }

        public static ActivityTemplateDTO Publish_To_Slack_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO
            {
                Id = Guid.NewGuid(),
                Name = "Publish_To_Slack_TEST",
                Label = "Publish To Slack",
                Category = ActivityCategory.Forwarders,
                Version = "1",
                Terminal = new TerminalDTO
                {
                    AuthenticationType = AuthenticationType.Internal
                }
            };
        }

        public static AuthorizationTokenDTO Slack_AuthToken()
        {
            return new AuthorizationTokenDTO
            {
                Token = @"xoxp-9815816992-9816213634-14997343526-d99a1c9198"
            };
        }


        internal static IEnumerable<FieldDTO> SlackEventFields()
        {
            return new List<FieldDTO>
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

        public static Fr8DataDTO Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO(bool isAuthToken)
        {
            var activityTemplate = Monitor_Channel_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO
            {
                Id = Guid.NewGuid(),
                Label = "Monitor_Channel",
                AuthToken = isAuthToken ? Slack_AuthToken() : null,
                ActivityTemplate = activityTemplate
            };
            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static Fr8DataDTO Publish_To_Slack_v1_InitialConfiguration_Fr8DataDTO(bool isAuthToken = true)
        {
            var activityTemplate = Publish_To_Slack_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO
            {
                Id = Guid.NewGuid(),
                Label = "Selected_Slack_Channel",
                AuthToken = isAuthToken ? Slack_AuthToken() : null,
                ActivityTemplate = activityTemplate
            };
            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
    }
}
