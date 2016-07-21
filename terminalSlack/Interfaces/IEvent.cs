using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;

namespace terminalSlack.Interfaces
{
    public interface IEvent
    {
        Task<Crate> Process(string externalEventPayload);
    }
}
