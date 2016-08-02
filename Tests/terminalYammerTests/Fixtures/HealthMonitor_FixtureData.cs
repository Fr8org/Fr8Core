using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalYammerTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {

        public static ActivityTemplateSummaryDTO Post_To_Yammer_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Post_To_Yammer_TEST",
                Version = "1"
            };
        }

        public static AuthorizationTokenDTO Yammer_AuthToken()
        {
            return new AuthorizationTokenDTO()
            {
                Token = @"2166748-bkXqpVW3pqTxFF5eTYuTzA"
            };
        }

        public static Fr8DataDTO Post_To_Yammer_v1_InitialConfiguration_Fr8DataDTO(bool isAuthToken = true)
        {
            var activityTemplate = Post_To_Yammer_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Selected Yammer Group",
                AuthToken = isAuthToken ? Yammer_AuthToken() : null,
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
    }
}
