using System;
using Data.Interfaces.DataTransferObjects;

namespace terminalTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static ActivityTemplateDTO MapFields_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "MapFields_TEST",
                Version = "1"
            };
        }

        public static ActionDTO MapFields_v1_InitialConfiguration_ActionDTO()
        {
            var activityTemplate = MapFields_v1_ActivityTemplate();

            return new ActionDTO()
            {
                Id = Guid.NewGuid(),
                Name = "MapFields",
                Label = "Map Fields",
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
        }
    }
}
