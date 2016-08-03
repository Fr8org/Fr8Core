using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Interfaces;
using Moq;
using StructureMap;

namespace terminalStatXTests.Fixtures
{
    partial class FixtureData
    {
        public static ActivityTemplateSummaryDTO MonitorStatChanges_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Monitor_Stat_Changes_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO Update_Stat_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Update_Stat_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO Create_Stat_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Create_Stat_TEST",
                Version = "1"
            };
        }

        public static Fr8DataDTO Update_Stat_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Update_Stat_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Update Stat",
                ActivityTemplate = activityTemplate,
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static Fr8DataDTO Create_Stat_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Create_Stat_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Create Stat",
                ActivityTemplate = activityTemplate,
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static Fr8DataDTO Monitor_Stat_Changes_InitialConfiguration_Fr8DataData()
        {
            var activityTemplate = MonitorStatChanges_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Update Stat",
                ActivityTemplate = activityTemplate,
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        private static readonly CrateManager CrateManager = new CrateManager();

        public static void ConfigureHubToReturnEmptyPayload()
        {
            var result = new PayloadDTO(Guid.Empty);
            using (var storage = CrateManager.GetUpdatableStorage(result))
            {
                storage.Add(Crate.FromContent(string.Empty, new OperationalStateCM()));
            }
            ObjectFactory.Container.GetInstance<Mock<IHubCommunicator>>().Setup(x => x.GetPayload(It.IsAny<Guid>()))
                               .Returns(Task.FromResult(result));
        }
    }
}
