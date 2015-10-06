namespace pluginAzureSqlServer.Interfaces
{
    public interface IEvent
    {
        void Process(string curExternalEventPayload);
    }
}
