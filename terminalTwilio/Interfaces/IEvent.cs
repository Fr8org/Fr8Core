namespace terminalTwilio.Services
{
    public interface IEvent
    {
        void Process(string curExternalEventPayload);
    }
}