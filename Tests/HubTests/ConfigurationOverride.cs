using System.Collections.Generic;
using Fr8.Infrastructure.Utilities.Configuration;

namespace HubTests
{
    public class ConfigurationOverride : IApplicationSettings
    {
        private readonly Dictionary<string, string> _settingsOverride = new Dictionary<string, string>();
        private readonly IApplicationSettings _base;

        public ConfigurationOverride(IApplicationSettings @base)
        {
            _base = @base;
        }

        public ConfigurationOverride Set(string key, string value)
        {
            _settingsOverride[key] = value;
            return this;
        } 

        public string GetSetting(string name)
        {
            string value;

            if (_settingsOverride.TryGetValue(name, out value))
            {
                return value;
            }

            return _base?.GetSetting(name);
        }
    }
}
