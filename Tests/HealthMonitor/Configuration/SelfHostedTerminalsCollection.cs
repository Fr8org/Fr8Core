using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace HealthMonitor.Configuration
{
    [ConfigurationCollection(
        typeof(SelfHostedTerminalsElement),
        AddItemName = "add",
        ClearItemsName = "clear",
        RemoveItemName = "remove"
    )]
    public class SelfHostedTerminalsCollection :
        ConfigurationElementCollection,
        IEnumerable<SelfHostedTerminalsElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SelfHostedTerminalsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var notifierCE = (SelfHostedTerminalsElement)element;
            return notifierCE;
        }

        IEnumerator<SelfHostedTerminalsElement>
            IEnumerable<SelfHostedTerminalsElement>.GetEnumerator()
        {
            return Enumerable.Range(0, this.Count)
                .Select(x => (SelfHostedTerminalsElement)BaseGet(x))
                .GetEnumerator();
        }
    }


    public class SelfHostedTerminalsElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsKey = true, IsRequired = true)]
        public string Type
        {
            get { return (string)base["type"]; }
            set { base["type"] = value; }
        }
        [ConfigurationProperty("url", IsKey = false, IsRequired = true)]
        public string Url
        {
            get { return (string)base["url"]; }
            set { base["url"] = value; }
        }
    }
}
