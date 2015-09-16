namespace pluginDocuSign.Services
{
    public interface IDocuSignEvent
    {
        void Process(string curEventPayload);
    }
}