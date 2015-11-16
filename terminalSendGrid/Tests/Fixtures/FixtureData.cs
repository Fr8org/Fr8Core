using System;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;

namespace terminalSendGrid.Tests.Fixtures
{
    public class FixtureData
    {
        public static ActionDO ConfigureSendEmailViaSendGridAction()
        {
            var actionTemplate = SendEmailViaSendGridActionTemplateDTO();

            var actionDO = new ActionDO()
            {
                Name = "testaction",
                Id = 333,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
                CrateStorage = ""
            };

            return actionDO;
        }

        public static Guid TestContainerGuid()
        {
            return new Guid("70790811-3394-4B5B-9841-F26A7BE35163");
        }

        public static ContainerDO TestContainer()
        {
            var containerDO = new ContainerDO();
            containerDO.Id = TestContainerGuid();
            containerDO.ContainerState = 1;
            return containerDO;
        }

        public static ActivityTemplateDO SendEmailViaSendGridActionTemplateDTO()
        {
            return new ActivityTemplateDO
            {
                Id = 1,
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

        public static PayloadDTO CratePayloadDTOForSendEmailViaSendGridConfiguration
        {
            get
            {
                PayloadDTO payloadDTO = new PayloadDTO(PayloadDTO_ContainerId());
                return payloadDTO;
            }
        }
    }
}
