using System.Configuration;

namespace Utilities.Configuration.Azure
{
    public class ConfigurationManagerApplicationSettings : IApplicationSettings
    {
        public string GetSetting(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }
}
