using System.Collections.Generic;
using System.Configuration;

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
    }
}
