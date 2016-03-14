using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace HealthMonitor.Configuration
{
    [ConfigurationCollection(
        typeof(SelfHostedAppsElement),
        AddItemName = "add",
        ClearItemsName = "clear",
        RemoveItemName = "remove"
    )]
    public class SelfHostedTerminalsCollection :
        ConfigurationElementCollection,
        IEnumerable<SelfHostedAppsElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SelfHostedAppsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var notifierCE = (SelfHostedAppsElement)element;
            return notifierCE;
        }

        IEnumerator<SelfHostedAppsElement>
            IEnumerable<SelfHostedAppsElement>.GetEnumerator()
        {
            return Enumerable.Range(0, this.Count)
                .Select(x => (SelfHostedAppsElement)BaseGet(x))
                .GetEnumerator();
        }
    }


    public class SelfHostedAppsElement : ConfigurationElement
    {
        private string _endpoint;

        [ConfigurationProperty("type", IsKey = true, IsRequired = true)]
        public string Type
        {
            get { return (string)base["type"]; }
            set { base["type"] = value; }
        }
        [ConfigurationProperty("name", IsKey = false, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        //This property is not mapped to a configuration element
        public string Endpoint
        {
            get { return _endpoint; }
            set { _endpoint = value; }
        }
    }
}
