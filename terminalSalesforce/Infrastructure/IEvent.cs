using System.Threading.Tasks;
using Data.Crates;

namespace terminalSalesforce.Infrastructure
{
    public interface IEvent
    {
        /// <summary>
        /// Processes external event payload from the terminal
        /// </summary>
        Task<Crate> ProcessEvent(string curExternalEventPayload);
    }
}
