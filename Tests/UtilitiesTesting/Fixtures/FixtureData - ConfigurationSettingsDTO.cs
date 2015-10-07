using System.Collections.Generic;
using Core.Interfaces;
using Core.Services;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces.ManifestSchemas;

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
            var fieldDTO = new ControlsDefinitionDTO(
                name: "connection_string", required: true, value: "", fieldLabel: "SQL Connection String");
            CrateStorageDTO curCrateStorage = new CrateStorageDTO();
            ICrate crate = ObjectFactory.GetInstance<ICrate>();
            curCrateStorage.CrateDTO.Add(crate.CreateStandardConfigurationControlsCrate("Configuration Data for WriteToAzureSqlServer", fieldDTO));
            return curCrateStorage;
        }

        public static ControlsDefinitionDTO TestConnectionString1()
        {
            return new ControlsDefinitionDTO
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

        public static ControlsDefinitionDTO TestConnectionStringFieldDefinition()
        {
            return new TextBlockFieldDTO()
            {
                Label = "SQL Connection String",
                Name = "connection_string",
                Required = true,
                Events = new List<FieldEvent>() { new FieldEvent("onChange", "requestConfig") }
            };
        }

        public static ActionDO TestConfigurationSettingsDTO1()
        {
            ActionDO curAction = FixtureData.TestAction1();

            //create connection string value crates with a vald connection string
            var connectionStringCrate = FixtureData.TestCrateStorage();
            var connectionStringFields = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(connectionStringCrate.CrateDTO[0].Contents);
            connectionStringFields.Controls[0].Value =
                @"Data Source=s79ifqsqga.database.windows.net;Initial Catalog=demodb_health;User ID=alexeddodb;Password=Thales89";
            connectionStringCrate.CrateDTO[0].Contents = JsonConvert.SerializeObject(connectionStringFields);
            curAction.CrateStorage = JsonConvert.SerializeObject(connectionStringCrate);
            return curAction;

        }
    }
}
