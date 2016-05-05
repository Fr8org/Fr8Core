using System;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using Data.Crates;
using Data.Interfaces.Manifests;

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

        public static PayloadDTO PayloadWithOnlyOperationalState()
        {
            var result = new PayloadDTO(Guid.NewGuid());
            using (var storage = new CrateManager().GetUpdatableStorage(result))
            {
                storage.Add(Crate.FromContent("Operational State", new OperationalStateCM()));
            }
            return result;
        }

        public static ActivityTemplateDTO GetDataFromFr8Warehouse_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "GetDataFromFr8Warehouse_TEST",
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
