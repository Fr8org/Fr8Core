using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json.Linq;

namespace Core.PluginRegistrations
{
    public class AzureSqlPluginRegistration : BasePluginRegistration
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

        public override IEnumerable<string> GetAvailableActions()
        {
            var curAvailableActions = new List<string> {"Write To Azure Sql Server"};

            return curAvailableActions;
        }

        public override JObject GetConfigurationSettings()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<string> GetFieldMappingTargets(string curActionName, string configUiData)
        {
            throw new System.NotImplementedException();
        }
    }
}
