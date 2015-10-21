namespace terminalSlack.Interfaces
{
    public interface IEvent
    {
        void Process(string externalEventPayload);
    }
}
