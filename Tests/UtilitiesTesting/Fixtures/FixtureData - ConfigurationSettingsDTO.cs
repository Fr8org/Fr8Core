using System.Collections.Generic;
using System.Linq;
using Data.Control;
using Data.Crates;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Services;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static CrateStorage TestConfigurationSettings_healthdemo()
        {
            return CrateStorageDTO();
        }

        public static CrateStorage CrateStorageDTO()
        {
            var fieldDTO = new TextBox();
            fieldDTO.Name = "connection_string";
            fieldDTO.Required = true;
            fieldDTO.Label = "SQL Connection String";

            CrateStorage curCrateStorage = new CrateStorage();
            ICrateManager crate = ObjectFactory.GetInstance<ICrateManager>();
            curCrateStorage.Add(crate.CreateStandardConfigurationControlsCrate("Configuration Data for WriteToAzureSqlServer", fieldDTO));
            return curCrateStorage;
        }

        public static ControlDefinitionDTO TestConnectionString1()
        {
            return new TextBlock()
            {
                Name = "Connection_String",
                Value = @"Server = tcp:s79ifqsqga.database.windows.net,1433; Database = demodb_health; User ID = alexeddodb@s79ifqsqga; Password = Thales89; Trusted_Connection = False; Encrypt = True; Connection Timeout = 30; "
            };
        }

        public static ControlDefinitionDTO TestConnectionString2()
        {
            return new TextBlock()
            {
                Name = "Connection_String",
                Value = @"Server = tcp:s79ifqsqga.database.windows.net,1433; Database = demodb_health_test; User ID = IntegrationTest@s79ifqsqga; Password = thmxsGv2Jqo; Trusted_Connection = False; Encrypt = True; Connection Timeout = 30; "
            };
        }

        public static CrateStorage TestCrateStorage()
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

        public static ActionDO TestConfigurationSettingsDTO1()
        {
            ActionDO curAction = FixtureData.TestAction1();
            ICrateManager crate = ObjectFactory.GetInstance<ICrateManager>();

            //create connection string value crates with a vald connection string
            
            using (var updater = crate.UpdateStorage(curAction))
            {
               updater.CrateStorage = TestCrateStorage();

               var connectionStringFields = updater.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                
                connectionStringFields.Content.Controls[0].Value = @"Data Source=s79ifqsqga.database.windows.net;Initial Catalog=demodb_health;User ID=alexeddodb;Password=Thales89";
            }
            
            return curAction;

        }
    }
}
