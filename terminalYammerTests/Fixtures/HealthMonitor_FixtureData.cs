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

        public static ActivityDTO Post_To_Yammer_v1_InitialConfiguration_ActionDTO(bool isAuthToken = true)
        {
            var activityTemplate = Post_To_Yammer_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Selected Yammer Group",
                AuthToken = isAuthToken ? Yammer_AuthToken() : null,
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }
    }
}
