using Data.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace Core.PluginRegistrations
{
    public class AzureSqlServerPluginRegistration_v1 : BasePluginRegistration
    {
        public const string BaseUrlKey = "AzureSql.BaseUrl";


        public string BaseUrl
        {
            get
            {
                return ConfigurationManager.AppSettings[BaseUrlKey];
            }
        }

        public IEnumerable<string> AvailableCommands
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
