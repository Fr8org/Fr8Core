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
        public static ActivityTemplateDTO MonitorStatChanges_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Monitor_Stat_Changes_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO Update_Stat_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Update_Stat_TEST",
                Version = "1"
            };
        }

        public static ActivityTemplateDTO MonitorFr8Event_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Post_To_Timeline_TEST",
                Version = "1"
            };
        }

        public static AuthorizationTokenDTO StatX_AuthToken()
        {
            return new AuthorizationTokenDTO()
            {
                Token = @"{\'apiKey\':\'statx_install_a06b7406-f570-4f62-9a39-970376c30b21\',\'authToken\':\'m82Janb2UqB3s/Eq1MikYAiyBMaUehmDAtJ08iNCcCg=\'}"
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
                AuthToken = StatX_AuthToken()
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static Fr8DataDTO Monitor_Stat_Changes_InitialConfiguration_Fr8DataData()
        {
            var activityTemplate = Update_Stat_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Update Stat",
                ActivityTemplate = activityTemplate,
                AuthToken = StatX_AuthToken()
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
