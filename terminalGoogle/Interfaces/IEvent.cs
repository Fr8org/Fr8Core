using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;

namespace terminalGoogle.Interfaces
{
    public interface IEvent
    {
        Task<Crate> Process(string externalEventPayload);
    }
}
