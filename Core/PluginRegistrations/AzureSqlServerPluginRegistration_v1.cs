using Data.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Core.PluginRegistrations
{
    public class AzureSqlServerPluginRegistration_v1 : BasePluginRegistration
    {
        public const string baseUrl = "AzureSql.BaseUrl";
        private const string availableActions = @"[{ ""ActionType"" : ""Write"" , ""Version"": ""1.0""}]";

        public AzureSqlServerPluginRegistration_v1()
            : base(availableActions, baseUrl)
        {

        }

        public string GetConfigurationSettings(ActionRegistrationDO curActionRegistrationDO)
        {
            return "{\"configurationSettings\":[{\"textField\": {\"name\": \"connection_string\",\"required\":true,\"value\":\"\",\"fieldLabel\":\"SQL Connection String\",}}]}";
        }
    }
}
