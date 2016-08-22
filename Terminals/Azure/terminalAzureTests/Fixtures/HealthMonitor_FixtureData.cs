using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalAzureTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static ActivityTemplateSummaryDTO Write_To_Sql_Server_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Write_To_Sql_Server_TEST",
                Version = "1"
            };
        }
        

        public static Fr8DataDTO Write_To_Sql_Server_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Write_To_Sql_Server_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Write To Sql Server",
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO
            {
                ActivityDTO = activityDTO
            };
        }
    }
}
