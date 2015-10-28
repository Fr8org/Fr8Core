using System.Collections.Generic;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Services;

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
            var fieldDTO = new TextBoxControlDefinitionDTO();
            fieldDTO.Name = "connection_string";
            fieldDTO.Required = true;
            fieldDTO.Label = "SQL Connection String";

            CrateStorageDTO curCrateStorage = new CrateStorageDTO();
            ICrateManager crate = ObjectFactory.GetInstance<ICrateManager>();
            curCrateStorage.CrateDTO.Add(crate.CreateStandardConfigurationControlsCrate("Configuration Data for WriteToAzureSqlServer", fieldDTO));
            return curCrateStorage;
        }

        public static ControlDefinitionDTO TestConnectionString1()
        {
            return new TextBlockControlDefinitionDTO()
            {
                Name = "Connection_String",
                Value = @"Server = tcp:s79ifqsqga.database.windows.net,1433; Database = demodb_health; User ID = alexeddodb@s79ifqsqga; Password = Thales89; Trusted_Connection = False; Encrypt = True; Connection Timeout = 30; "
            };
        }

        public static CrateStorageDTO TestCrateStorage()
        {
            ICrateManager crate = ObjectFactory.GetInstance<ICrateManager>();
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

        public static ControlDefinitionDTO TestConnectionStringFieldDefinition()
        {
            return new TextBlockControlDefinitionDTO()
            {
                Label = "SQL Connection String",
                Name = "connection_string",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };
        }

        public static ActionDO TestConfigurationSettingsDTO1()
        {
            ActionDO curAction = FixtureData.TestAction1();

            //create connection string value crates with a vald connection string
            var connectionStringCrate = FixtureData.TestCrateStorage();
            var connectionStringFields = JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(connectionStringCrate.CrateDTO[0].Contents);
            connectionStringFields.Controls[0].Value =
                @"Data Source=s79ifqsqga.database.windows.net;Initial Catalog=demodb_health;User ID=alexeddodb;Password=Thales89";
            connectionStringCrate.CrateDTO[0].Contents = JsonConvert.SerializeObject(connectionStringFields);
            curAction.CrateStorage = JsonConvert.SerializeObject(connectionStringCrate);
            return curAction;

        }
    }
}
