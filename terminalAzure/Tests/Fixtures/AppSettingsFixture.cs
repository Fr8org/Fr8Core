using System.Collections.Generic;
using Utilities.Configuration;

namespace terminalAzure.Tests.Fixtures
{
    public class AppSettingsFixture : IApplicationSettings
    {
        private static readonly Dictionary<string, string> _settings
            = new Dictionary<string, string>()
        {
            { "CoreWebServerUrl", "http://localhost:30643/" }
        };

        public string GetSetting(string name)
        {
            string returnValue;
            if (!_settings.TryGetValue(name, out returnValue))
            {
                return null;
            }

            return returnValue;
        }
    }
}