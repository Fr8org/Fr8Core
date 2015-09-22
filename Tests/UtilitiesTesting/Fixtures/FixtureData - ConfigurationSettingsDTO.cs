using System.Collections.Generic;
using Core.Interfaces;
using Core.Services;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using StructureMap;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static CrateStorageDTO TestConfigurationSettings_healthdemo()
        {
            return CrateStorageDTO();
        }

        public static CrateStorageDTO CrateStorageDTO()
        {
            var fieldDTO = new FieldDefinitionDTO(
                name: "connection_string", required: true, value: "", fieldLabel: "SQL Connection String");
            CrateStorageDTO curCrateStorage = new CrateStorageDTO();
            ICrate crate = ObjectFactory.GetInstance<ICrate>();
            curCrateStorage.CrateDTO.Add(crate.CreateStandardConfigurationControlsCrate("Configuration Data for WriteToAzureSqlServer", fieldDTO));
            return curCrateStorage;
        }

        public static FieldDefinitionDTO TestConnectionString1()
        {
            return new FieldDefinitionDTO
            {
                Name = "Connection_String",
                Value = @"Server = tcp:s79ifqsqga.database.windows.net,1433; Database = demodb_health; User ID = alexeddodb@s79ifqsqga; Password = Thales89; Trusted_Connection = False; Encrypt = True; Connection Timeout = 30; "

            };
        }

        public static CrateStorageDTO TestCrateStorage()
        {
            ICrate crate = ObjectFactory.GetInstance<ICrate>();
            var curConfigurationStore = new CrateStorageDTO
            {
                //this needs to be updated to hold Crates instead of FieldDefinitionDTO
                CrateDTO = new List<CrateDTO>
                {
                    crate.CreateStandardConfigurationControlsCrate("AzureSqlServer Design-Time Fields", TestConnectionStringFieldDefinition())
                }
            };

            return curConfigurationStore;
        }

        public static FieldDefinitionDTO TestConnectionStringFieldDefinition()
        {
            return new FieldDefinitionDTO()
            {
                FieldLabel = "SQL Connection String",
                Type = "textField",
                Name = "connection_string",
                Required = true,
                Events = new List<FieldEvent>() {new FieldEvent("onChange", "requestConfig")}
            };
        }
    }
}
