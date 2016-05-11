using System.Threading.Tasks;
using Fr8Data.Crates;


namespace terminalGoogle.Infrastructure
{
    public interface IEvent
    {
        Task<Crate> Process(string externalEventPayload);
    }
}
