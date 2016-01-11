using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace HealthMonitor.Configuration
{
    [ConfigurationCollection(
        typeof(NotifierConfigurationElement),
        AddItemName = "add",
        ClearItemsName = "clear",
        RemoveItemName = "remove"
    )]
    public class NotifiersConfigurationCollection :
        ConfigurationElementCollection,
        IEnumerable<NotifierConfigurationElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new NotifierConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var notifierCE = (NotifierConfigurationElement)element;
            return notifierCE.Email;
        }

        IEnumerator<NotifierConfigurationElement>
            IEnumerable<NotifierConfigurationElement>.GetEnumerator()
        {
            return Enumerable.Range(0, this.Count)
                .Select(x => (NotifierConfigurationElement)BaseGet(x))
                .GetEnumerator();
        }
    }


    public class NotifierConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("email", IsKey = true, IsRequired = true)]
        public string Email
        {
            get { return (string)base["email"]; }
            set { base["email"] = value; }
        }
    }
}