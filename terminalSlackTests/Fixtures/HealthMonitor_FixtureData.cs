using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Hub.Services;

namespace terminalSlackTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static ActionDTO Monitor_Channel_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Monitor_Channel_v1_ActivityTemplate();

            return new ActionDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Monitor_Channel",
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
                Token = @"xoxp-7518126694-11767329056-15941434949-2ca03232a3"
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
               new FieldDTO("channel_id", "C0BU4CH25"),
               new FieldDTO("channel_name", "slack - plugin - test"),
               new FieldDTO("timestamp", "1449594901.000014"),
               new FieldDTO("user_id"," U0BNK9P1N"),
               new FieldDTO("user_name", "sergeyp"),
               new FieldDTO("text", "test")
           };
        }
    }
}
