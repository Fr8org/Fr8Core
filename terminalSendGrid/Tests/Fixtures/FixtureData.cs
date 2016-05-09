using System;
using Data.Entities;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using Newtonsoft.Json;

namespace terminalSendGrid.Tests.Fixtures
{
    public class FixtureData
    {
        public static Guid TestGuid_Id_333()
        {
            return new Guid("A0287C2A-28D3-48C5-8CAC-26FE27E8EA9B");
        }

        public static ActivityDO ConfigureSendEmailViaSendGridActivity()
        {
            var actionTemplate = SendEmailViaSendGridActivityTemplateDTO();

            var activityDO = new ActivityDO()
            {
                Id = TestGuid_Id_333(),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
                CrateStorage = ""
            };

            return activityDO;
        }

        public static Guid TestContainerGuid()
        {
            return new Guid("70790811-3394-4B5B-9841-F26A7BE35163");
        }

        public static ContainerDO TestContainer()
        {
            var containerDO = new ContainerDO();
            containerDO.Id = TestContainerGuid();
            containerDO.State = 1;
            return containerDO;
        }

        public static ActivityTemplateDO SendEmailViaSendGridActivityTemplateDTO()
        {
            return new ActivityTemplateDO
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

        public static PayloadDTO CratePayloadDTOForSendEmailViaSendGridConfiguration
        {
            get
            {
                var payloadDTO = new PayloadDTO(PayloadDTO_ContainerId());
                using (var crateStorage = new CrateManager().GetUpdatableStorage(payloadDTO))
                {
                    var operationalStatus = new OperationalStateCM();
                    var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
                    crateStorage.Add(operationsCrate);
                }
                return payloadDTO;
            }
        }
    }
}
