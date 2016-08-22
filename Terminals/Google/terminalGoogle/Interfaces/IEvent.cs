using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using StructureMap;

namespace terminalGoogle.Interfaces
{
    public interface IEvent
    {
        Task<Crate> Process(IContainer container, string externalEventPayload);
    }
}
