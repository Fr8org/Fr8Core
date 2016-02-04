
using System;
using Data.Interfaces.DataTransferObjects;
using Data.States;

namespace terminalPapertrailTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static ActivityTemplateDTO Write_To_Log_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Write_To_Log_TEST",
                Version = "1"
            };
        }

        public static ActivityDTO Write_To_Log_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = Write_To_Log_v1_ActivityTemplate();

            return new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Write To Log",
                AuthToken = null,
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }
    }
}
