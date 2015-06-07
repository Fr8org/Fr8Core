using System;
using System.Configuration;

namespace Configuration
{
    public class DaemonConfig : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
        }

        [ConfigurationProperty("initClass", IsRequired = true)]
        public string InitClass
        {
            get { return (string)this["initClass"]; }
        }

        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
        }
    }

    public class Daemons : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DaemonConfig();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DaemonConfig)element).Name;
        }
    }

    public class DaemonSettings : ConfigurationSection
    {
        [ConfigurationProperty("daemons", IsRequired = true)]
        public Daemons Daemons
        {
            get { return (Daemons)this["daemons"]; }
        }

        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
        }

        [ConfigurationProperty("daemonAssemblyName", IsRequired = true)]
        public String DaemonAssemblyName
        {
            get { return (String)this["daemonAssemblyName"]; }
        }
    }
}
