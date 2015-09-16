namespace pluginDockyardCore.Services
{
    public interface IDockyardCoreEvent
    {
        void Process(string curEventPayload);
    }
}