using System.Collections.Generic;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.Manifests;
using StructureMap;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static void AddOperationalStateCrate(PayloadDTO payload)
        {
            using (var crateStorage = new CrateManager().GetUpdatableStorage(payload))
            {
                var operationalStatus = new OperationalStateCM();
                var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
                crateStorage.Add(operationsCrate);
            }
        }

        public static PayloadDTO PayloadDTO1()
        {
            List<FieldDTO> curFields = new List<FieldDTO>()
            {
                new FieldDTO() { Key = "EnvelopeId", Value = "EnvelopeIdValue" }
            };

            EventReportCM curEventReportMS = new EventReportCM();
            curEventReportMS.EventNames = "DocuSign Envelope Sent";
            curEventReportMS.Manufacturer = "DocuSign";
            curEventReportMS.EventPayload.Add(Crate.FromContent("Standard Event Report", new StandardPayloadDataCM(curFields)));
            var payload = new PayloadDTO(TestContainer_Id_1());

            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(payload))
            {
                crateStorage.Add(Crate.FromContent("Standard Event Report", curEventReportMS));
            }

            AddOperationalStateCrate(payload);

            return payload;
        }

        public static ICrateStorage PayloadDTO2()
        {
            var standardPayload = new StandardPayloadDataCM(
                new List<FieldDTO>()
                {
                    new FieldDTO() { Key = "EnvelopeId", Value = "EnvelopeIdValue" }
                }
            );

            var payload = new PayloadDTO(TestContainer_Id_49());
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();

            using (var crateStorage = crateManager.GetUpdatableStorage(payload))
            {
                crateStorage.Add(Crate.FromContent("Standard Payload Data", standardPayload));
            }

            AddOperationalStateCrate(payload);

            return crateManager.GetStorage(payload);
        }

    }
}