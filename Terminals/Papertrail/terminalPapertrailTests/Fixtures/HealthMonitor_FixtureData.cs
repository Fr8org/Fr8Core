using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalPapertrailTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static ActivityTemplateSummaryDTO Write_To_Log_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Write_To_Log_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO Write_To_Log_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Write_To_Log_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Write To Log",
                AuthToken = null,
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
    }
}
