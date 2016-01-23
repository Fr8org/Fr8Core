using System.Threading.Tasks;
using Data.Crates;

namespace terminalSlack.Interfaces
{
    public interface IEvent
    {
        Task<Crate> Process(string externalEventPayload);
    }
}
