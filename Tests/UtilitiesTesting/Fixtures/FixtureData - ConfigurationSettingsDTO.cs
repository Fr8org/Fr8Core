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
            string contents = "{ name: 'connection_string', required: true, value: '', fieldLabel: 'SQL Connection String' }";
            CrateStorageDTO curCrateStorage = new CrateStorageDTO();
            ICrate _crate = ObjectFactory.GetInstance<ICrate>();
            curCrateStorage.CrateDTO.Add(_crate.Create("Configuration Data for WriteToAzureSqlServer", contents));
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
            var fieldDefinitions = new List<FieldDefinitionDTO>() 
            {
                TestConnectionStringFieldDefinition()
            };

            var curConfigurationStore = new CrateStorageDTO
            {
                //this needs to be updated to hold Crates instead of FieldDefinitionDTO
                CrateDTO = new List<CrateDTO>
                {
                    new CrateDTO
                    {
                        Label = "AzureSqlServer Design-Time Fields",
                        Contents = JsonConvert.SerializeObject(fieldDefinitions),
                        ManifestType = "Standard Configuration Controls"
                    }
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
