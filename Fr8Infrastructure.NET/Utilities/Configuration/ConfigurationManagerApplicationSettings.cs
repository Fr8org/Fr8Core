using System.Configuration;

namespace fr8.Infrastructure.Utilities.Configuration
{
    public class ConfigurationManagerApplicationSettings : IApplicationSettings
    {
        public string GetSetting(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }
}
