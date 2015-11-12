using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using System.Xml;
using System.Xml.Linq;
using Data.Crates;

namespace terminalSalesforce.Infrastructure
{
    public interface IEvent
    {
        /// <summary>
        /// Processes external event payload from the terminal
        /// </summary>
        Crate ProcessEvent(string curExternalEventPayload);
    }
}
