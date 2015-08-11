using Data.Entities;
using System.Collections.Generic;
using System.Configuration;

namespace Core.PluginRegistrations
{
    public class AzureSqlServerPluginRegistration_v1 : BasePluginRegistration
    {
        public const string BaseUrlKey = "AzureSql.BaseUrl";


        public override string BaseUrl
        {
            get
            {
                return ConfigurationManager.AppSettings[BaseUrlKey];
            }
        }

        public override IEnumerable<string> AvailableCommands
        {
            get
            {
                return new[]
                {
                    "writeSQL"
                };
            }
        }

        public string GetConfigurationSettings(ActionRegistrationDO curActionRegistrationDO)
        {
            return "{\"configurationSettings\":[{\"textField\": {\"name\": \"connection_string\",\"required\":true,\"value\":\"\",\"fieldLabel\":\"SQL Connection String\",}}]}";
        }

    }
}
