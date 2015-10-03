using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Newtonsoft.Json;
using StructureMap;
using System.Collections.Generic;
using System;
using Data.Interfaces.ManifestSchemas;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {


        public static List<CrateDTO> TestCrateDTO1()
        {
            List<FieldDTO> fields = new List<FieldDTO>();
            fields.Add(new FieldDTO() { Key = "Medical_Form_v1", Value = Guid.NewGuid().ToString() });
            fields.Add(new FieldDTO() { Key = "Medical_Form_v2", Value = Guid.NewGuid().ToString() });

            CrateDTO curCrateDTO = new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Available Templates",
                Contents = JsonConvert.SerializeObject(new StandardDesignTimeFieldsMS() { Fields = fields }),
                ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME,
                ManifestId = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_ID

            };

            return new List<CrateDTO>() { curCrateDTO };

        }

    }
}