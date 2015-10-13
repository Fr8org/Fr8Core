﻿using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static PayloadDTO PayloadDTO1()
        {
            List<FieldDTO> curFields = new List<FieldDTO>() { new FieldDTO() { Key = "EnvelopeId", Value = "EnvelopeIdValue" } };

            EventReportCM curEventReportMS = new EventReportCM();
            curEventReportMS.EventNames = "DocuSign Envelope Sent";
            curEventReportMS.EventPayload.Add(new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Standard Event Report",
                ManifestType = CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME,
                ManifestId = CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID,
                Contents = JsonConvert.SerializeObject(curFields)

            });

            CrateDTO curCrateDTO = new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Standard Event Report",
                ManifestType = "Standard Event Report",
                Contents = JsonConvert.SerializeObject(curEventReportMS)
            };

            CrateStorageDTO curCrateStorageDTO = new CrateStorageDTO();
            curCrateStorageDTO.CrateDTO.Add(curCrateDTO);
            var curStorage = JsonConvert.SerializeObject(curCrateStorageDTO);


            return new PayloadDTO(curStorage, 1);
        }

        public static PayloadDTO PayloadDTO2()
        {
            List<FieldDTO> curFields = new List<FieldDTO>() { new FieldDTO() { Key = "EnvelopeId", Value = "EnvelopeIdValue" } };

            CrateDTO curCrateDTO = new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Standard Payload Data",
                ManifestType = CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME,
                Contents = JsonConvert.SerializeObject(curFields)
            };

            CrateStorageDTO curCrateStorageDTO = new CrateStorageDTO();
            curCrateStorageDTO.CrateDTO.Add(curCrateDTO);
            var curStorage = JsonConvert.SerializeObject(curCrateStorageDTO);


            return new PayloadDTO(curStorage, 49);
        }

    }
}