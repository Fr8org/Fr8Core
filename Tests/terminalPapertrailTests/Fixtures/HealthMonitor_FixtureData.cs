using System;
using fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalPapertrailTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static ActivityTemplateDTO Write_To_Log_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
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
