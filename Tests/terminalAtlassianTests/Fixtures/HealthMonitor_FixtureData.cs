using System;
using Newtonsoft.Json;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

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
                Password = "yakima29"
            };

            return new AuthorizationTokenDTO()
            {
                Token = JsonConvert.SerializeObject(curCredentialsDTO)
            };
        }

        public static ActivityTemplateDTO Get_Jira_Issue_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Get_Jira_Issue_TEST",
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
    }
}
