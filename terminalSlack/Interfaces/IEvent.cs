using Data.Crates;

namespace terminalSlack.Interfaces
{
    public interface IEvent
    {
        Crate Process(string externalEventPayload);
    }
}
