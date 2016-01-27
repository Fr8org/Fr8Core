﻿using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
﻿using Data.Crates;
﻿using Hub.Managers;
﻿using StructureMap;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static void AddOperationalStateCrate(PayloadDTO payload)
        {
            using (var updater = new CrateManager().UpdateStorage(payload))
            {
                var operationalStatus = new OperationalStateCM();
                var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
                updater.CrateStorage.Add(operationsCrate);
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

            using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(payload))
            {
                updater.CrateStorage.Add(Crate.FromContent("Standard Event Report", curEventReportMS));
            }

            AddOperationalStateCrate(payload);

            return payload;
        }

        public static PayloadDTO PayloadDTO2()
        {
            var standardPayload = new StandardPayloadDataCM(
                new List<FieldDTO>()
                {
                    new FieldDTO() { Key = "EnvelopeId", Value = "EnvelopeIdValue" }
                }
            );

            var payload = new PayloadDTO(TestContainer_Id_49());

            using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(payload))
            {
                updater.CrateStorage.Add(Crate.FromContent("Standard Payload Data", standardPayload));
            }

            AddOperationalStateCrate(payload);

            return payload;
        }

    }
}