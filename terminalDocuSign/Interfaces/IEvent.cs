using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using StructureMap;

namespace terminalDocuSign.Interfaces
{
    public interface IEvent
    {
        /// <summary>
        /// Processes external event payload from the Terminal
        /// </summary>
        Task<Crate> Process(IContainer container, string curExternalEventPayload);
    }
}