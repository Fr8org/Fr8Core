using System.Threading.Tasks;
using Fr8Data.Crates;

namespace terminalSlack.Interfaces
{
    public interface IEvent
    {
        Task<Crate> Process(string externalEventPayload);
    }
}
