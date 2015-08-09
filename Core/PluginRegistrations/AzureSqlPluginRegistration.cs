using System.Collections.Generic;
using System.Configuration;

namespace Core.PluginRegistrations
{
    public class AzureSqlPluginRegistration : BasePluginRegistration
    {

        public AzureSqlPluginRegistration()
            : base(ConfigurationManager.AppSettings[BaseUrlKey], new[] { "writeSQL" })
        {

        }

        public const string BaseUrlKey = "AzureSql.BaseUrl";



    }
}
