using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalBox.Infrastructure
{
    public static class BoxHelpers
    {
        public static string ClientId = CloudConfigurationManager.GetSetting("BoxClientId");
        public static string Secret = CloudConfigurationManager.GetSetting("BoxSecret");
        public static string RedirectUri = CloudConfigurationManager.GetSetting("BoxCallbackUrlsDomain");
    }
}