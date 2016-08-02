using System;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;

namespace terminalTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static ActivityTemplateSummaryDTO MapFields_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
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

        public static ContainerExecutionContext ExecutionContextWithOnlyOperationalState()
        {
            var result = new ContainerExecutionContext()
            {
                ContainerId = Guid.NewGuid(),
                PayloadStorage = new CrateStorage(Crate.FromContent("Operational State", new OperationalStateCM()))
            };

            return result;
        }

        public static ActivityTemplateSummaryDTO GetDataFromFr8Warehouse_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Get_Data_From_Fr8_Warehouse_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO GetDataFromFr8Warehouse_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = GetDataFromFr8Warehouse_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Get Data From Fr8 Warehouse",
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
    }
}
