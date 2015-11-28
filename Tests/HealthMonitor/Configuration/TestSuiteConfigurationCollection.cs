using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace HealthMonitor.Configuration
{
    [ConfigurationCollection(
        typeof(TestSuiteConfigurationElement),
        AddItemName = "add",
        ClearItemsName = "clear",
        RemoveItemName = "remove"
    )]
    public class TestSuiteConfigurationCollection :
        ConfigurationElementCollection,
        IEnumerable<TestSuiteConfigurationElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new TestSuiteConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var testSuiteCE = (TestSuiteConfigurationElement)element;
            return testSuiteCE.Type;
        }

        IEnumerator<TestSuiteConfigurationElement>
            IEnumerable<TestSuiteConfigurationElement>.GetEnumerator()
        {
            return Enumerable.Range(0, this.Count)
                .Select(x => (TestSuiteConfigurationElement)BaseGet(x))
                .GetEnumerator();
        }
    }


    public class TestSuiteConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsKey = true, IsRequired = true)]
        public string Type
        {
            get { return (string)base["type"]; }
            set { base["type"] = value; }
        }
    }
}
