using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using StructureMap;

namespace terminalFacebook.Interfaces
{
    public interface IEvent
    {
        /// <summary>
        /// Processes external event payload from the Terminal
        /// </summary>
        Task<List<Crate>> ProcessUserEvents(IContainer container, string curExternalEventPayload);
    }
}