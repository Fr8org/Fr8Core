using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;

namespace terminalZendesk.Interfaces
{
    public interface IEvent
    {
        Task<Crate> Process(string externalEventPayload);
    }
}
