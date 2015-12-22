using System.Collections.Generic;
using Utilities.Configuration;

namespace terminalDocuSign.Tests.Fixtures
{
    public class AppSettingsFixture : IApplicationSettings
    {
        private static readonly Dictionary<string, string> _settings
            = new Dictionary<string, string>()
        {
            { "CoreWebServerUrl", "http://localhost:30643/" },
            { "DocuSignLoginEmail", "64684b41-bdfd-4121-8f81-c825a6a03582" },
            { "DocuSignLoginPassword", "grolier34" },
            { "DocuSignIntegratorKey", "TEST-4057de18-b5ae-43be-a408-565be7755cef" },
            { "environment", "https://demo.docusign.net/" },
            { "endpoint", "https://demo.docusign.net/restapi/v2/" },
            { "BaseUrl", "https://demo.docusign.net/restapi/v2/accounts/1026172/" }
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