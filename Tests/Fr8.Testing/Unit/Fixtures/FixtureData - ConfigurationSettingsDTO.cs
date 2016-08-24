using System.Collections.Generic;
using System.Linq;
using StructureMap;
using Data.Entities;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Hub.Managers;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;

namespace Fr8.Testing.Unit.Fixtures
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

            curCrateStorage.Add("Configuration Data for WriteToAzureSqlServer", new StandardConfigurationControlsCM(fieldDTO));

            return curCrateStorage;
        }

        public static ControlDefinitionDTO TestConnectionString2()
        {
            return new TextBox()
            {
                Name = "Connection_String",
                Value = @"Data Source=tcp:fr8vmbuilddb.westus.cloudapp.azure.com,1433;Initial Catalog=demodb_health_test;User ID=builddbadmin;Password=Pangea9602;"
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
    }
}
