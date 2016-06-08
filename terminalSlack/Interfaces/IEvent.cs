using System.Threading.Tasks;
using fr8.Infrastructure.Data.Crates;

namespace terminalSlack.Interfaces
{
    public interface IEvent
    {
        Task<Crate> Process(string externalEventPayload);
    }
}
