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
                Id = Guid.NewGuid(),
                Name = "MapFields_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO MapFields_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = MapFields_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Map Fields",
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };

        }
    }
}
