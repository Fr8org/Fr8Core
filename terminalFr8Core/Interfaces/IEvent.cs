using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;

namespace terminalFr8Core.Interfaces
{
    public interface IEvent
    {
        /// <summary>
        /// Processes external event payload from the Terminal
        /// </summary>
        Task<Crate> Process(string curExternalEventPayload);
    }
}