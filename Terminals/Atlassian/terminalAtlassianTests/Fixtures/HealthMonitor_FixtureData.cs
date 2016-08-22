using System;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace terminalAtlassianTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static AuthorizationTokenDTO Jira_AuthToken()
        {
            var curCredentialsDTO = new CredentialsDTO()
            {
                Domain = "https://maginot.atlassian.net",
                Username = "fr8_atlassian_test",
                Password = "shoggoth34"
            };

            return new AuthorizationTokenDTO()
            {
                Token = JsonConvert.SerializeObject(curCredentialsDTO)
            };
        }

        public static ActivityTemplateSummaryDTO Get_Jira_Issue_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Get_Jira_Issue_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO Monitor_Jira_Events_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Monitor_Jira_Changes_v1_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO Get_Jira_Issue_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Get_Jira_Issue_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Get Jira Issue",
                AuthToken = Jira_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static Fr8DataDTO Monitor_Jira_Events_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Monitor_Jira_Events_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Monitor Jira Event",
                AuthToken = Jira_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
    }
}
