using Data.Interfaces.DataTransferObjects;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static ConfigurationSettingsDTO TestConfigurationSettings_healthdemo()
        {
            return new ConfigurationSettingsDTO
            {
                FieldDefinitions =  {
                    TestConnectionString1()
                }
            };
        }

        public static FieldDefinitionDTO TestConnectionString1()
        {
            return new FieldDefinitionDTO
            {
                Name = "Connection_String",
                Value = @"Server = tcp:s79ifqsqga.database.windows.net,1433; Database = dockyard_azure_db_server1; User ID = alexeddodb@s79ifqsqga; Password = Thales89; Trusted_Connection = False; Encrypt = True; Connection Timeout = 30; " 

            };
        }


    }
}
