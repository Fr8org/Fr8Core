using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using System.Xml;
using System.Xml.Linq;
namespace terminalSalesforce.Infrastructure
{
    public interface IEvent
    {
        /// <summary>
        /// Processes external event payload from the plugin
        /// </summary>
        CrateDTO ProcessEvent(string curExternalEventPayload);
    }
}
