using System.Collections.Generic;
using System.Linq;
using StructureMap;
using Data.Entities;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Managers;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static ICrateStorage TestConfigurationSettings_healthdemo()
        {
            return CrateStorageDTO();
        }

        public static ICrateStorage CrateStorageDTO()
        {
            var fieldDTO = new TextBox();
            fieldDTO.Name = "connection_string";
            fieldDTO.Required = true;
            fieldDTO.Label = "SQL Connection String";

            ICrateStorage curCrateStorage = new CrateStorage();
            ICrateManager crate = ObjectFactory.GetInstance<ICrateManager>();
            curCrateStorage.Add(crate.CreateStandardConfigurationControlsCrate("Configuration Data for WriteToAzureSqlServer", fieldDTO));
            return curCrateStorage;
        }

        public static ControlDefinitionDTO TestConnectionString2()
        {
            return new TextBox()
            {
                Name = "Connection_String",
                Value = @"Server = tcp:s79ifqsqga.database.windows.net,1433; Database = demodb_health_test; User ID = IntegrationTest@s79ifqsqga; Password = thmxsGv2Jqo; Trusted_Connection = False; Encrypt = True; Connection Timeout = 30; "
            };
        }

        public static ControlDefinitionDTO TestConnectionString3()
        {
            return new TextBlock()
            {
                Name = "Connection_String",
                Value = @"This is incorrect database connection string!"
            };
        }

        public static ICrateStorage TestCrateStorage()
        {
            ICrateManager crate = ObjectFactory.GetInstance<ICrateManager>();
            var curConfigurationStore = new CrateStorage
            {
                //this needs to be updated to hold Crates instead of FieldDefinitionDTO
               
                    crate.CreateStandardConfigurationControlsCrate("AzureSqlServer Design-Time Fields", TestConnectionStringFieldDefinition())
                
            };

            return curConfigurationStore;
        }

        public static ControlDefinitionDTO TestConnectionStringFieldDefinition()
        {
            return new TextBlock()
            {
                Label = "SQL Connection String",
                Name = "connection_string",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };
        }

        public static ActivityDO TestConfigurationSettingsDTO1()
        {
            ActivityDO curAction = FixtureData.TestActivity1();
            ICrateManager crate = ObjectFactory.GetInstance<ICrateManager>();

            //create connection string value crates with a vald connection string
            
            using (var crateStorage = crate.GetUpdatableStorage(curAction))
            {
                crateStorage.Replace(TestCrateStorage());

               var connectionStringFields = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                
                connectionStringFields.Content.Controls[0].Value = @"Data Source=s79ifqsqga.database.windows.net;Initial Catalog=demodb_health;User ID=alexeddodb;Password=Thales89";
            }
            
            return curAction;

        }
    }
}
