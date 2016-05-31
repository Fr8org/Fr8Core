using System;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using StructureMap;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

namespace terminalSendGridTests.Fixtures
{
    public class FixtureData
    {
        public static Guid TestGuid_Id_333()
        {
            return new Guid("A0287C2A-28D3-48C5-8CAC-26FE27E8EA9B");
        }

        public static ActivityPayload ConfigureSendEmailViaSendGridActivity()
        {
            var actionTemplate = SendEmailViaSendGridActivityTemplateDTO();

            var activityPayload = new ActivityPayload()
            {
                Id = TestGuid_Id_333(),
                //ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
                CrateStorage = new CrateStorage()
            };

            return activityPayload;
        }

        public static Guid TestContainerGuid()
        {
            return new Guid("70790811-3394-4B5B-9841-F26A7BE35163");
        }

        
        public static ContainerDTO TestContainer()
        {
            var containerDO = new ContainerDTO();
            containerDO.Id = TestContainerGuid();
            containerDO.State = 1;
            return containerDO;
        }

        public static ActivityTemplateDTO SendEmailViaSendGridActivityTemplateDTO()
        {
            return new ActivityTemplateDTO
            {
                Id = Guid.NewGuid(),
                Name = "Send Email Via SendGrid",
                Version = "1"
            };
        }

        public static CrateDTO CrateDTOForSendEmailViaSendGridConfiguration()
        {
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Configuration_Controls",
                Contents = "test contents",
                CreateTime = DateTime.Now,
                ManifestId = 1,
                ManifestType = "ManifestType",
                Manufacturer = new ManufacturerDTO(),
                ParentCrateId = "ParentCrateId"
            };
        }

        public static Guid PayloadDTO_ContainerId()
        {
            return new Guid("07EF735F-42F5-435E-8DC2-D039351463FD");
        }

        public static ContainerExecutionContext CrateExecutionContextForSendEmailViaSendGridConfiguration
        {
            get
            {
                var exeuctionContext = new ContainerExecutionContext
                {
                    ContainerId = PayloadDTO_ContainerId(),
                    PayloadStorage = new CrateStorage(Crate.FromContent("Operational Status", new OperationalStateCM()))
                };
                return exeuctionContext;
            }
        }
        public static ActivityContext TestActivityContext1()
        {
            var activityTemplateDTO = new ActivityTemplateDTO
            {
                Id = Guid.NewGuid(),
                Name = "Type1",
                Version = "1",
                Terminal = new TerminalDTO { Name = "TestTerminal"}
            };
            var activityPayload = new ActivityPayload
            {
                Id = Guid.NewGuid(),
                Name = "Type2",
                ActivityTemplate = activityTemplateDTO,
                CrateStorage = new CrateStorage()
            };
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = activityPayload
            };
            return activityContext;
        }
    }
}
