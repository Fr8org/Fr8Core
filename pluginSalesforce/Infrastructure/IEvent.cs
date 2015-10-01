using System.Xml;
using System.Xml.Linq;
namespace pluginSalesforce.Infrastructure
{
    public interface IEvent
    {
        /// <summary>
        /// Processes external event payload from the plugin
        /// </summary>
        void Process(string curExternalEventPayload);
    }
}
