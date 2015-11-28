using System.Configuration;

namespace HealthMonitor.Configuration
{
    public class HealthMonitorConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("testSuites", IsDefaultCollection = false)]
        public TestSuiteConfigurationCollection TestSuites
        {
            get
            {
                var collection = (TestSuiteConfigurationCollection)base["testSuites"];
                return collection;
            }
        }

        [ConfigurationProperty("notifiers", IsDefaultCollection = false)]
        public NotifiersConfigurationCollection Notifiers
        {
            get
            {
                var collection = (NotifiersConfigurationCollection)base["notifiers"];
                return collection;
            }
        }
    }
}
