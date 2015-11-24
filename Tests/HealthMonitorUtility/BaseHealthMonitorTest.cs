using System.Configuration;

namespace HealthMonitor.Utility
{
    public abstract class BaseHealthMonitorTest
    {
        public JsonRestClient JsonRestClient { get; set; }

        public BaseHealthMonitorTest()
        {
            JsonRestClient = new JsonRestClient();
        }

        public abstract string TerminalName { get; }

        public string GetTerminalUrl()
        {
            return ConfigurationManager.AppSettings[TerminalName + "Url"];
        }

        public string GetTerminalConfigureUrl()
        {
            return GetTerminalUrl() + "/actions/configure";
        }
    }
}
