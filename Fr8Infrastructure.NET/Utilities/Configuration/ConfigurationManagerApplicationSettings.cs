using System.Configuration;

namespace Fr8.Infrastructure.Utilities.Configuration
{
    public class ConfigurationManagerApplicationSettings : IApplicationSettings
    {
        public string GetSetting(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }
}
