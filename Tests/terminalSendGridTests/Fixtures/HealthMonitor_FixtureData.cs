using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalSendGridTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static ActivityTemplateSummaryDTO SendEmailViaSendGrid_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO
            {
                Name = "Send_Email_Via_SendGrid_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO SendEmailViaSendGrid_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = SendEmailViaSendGrid_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Send Email",
                AuthToken = null,
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

    }
}
