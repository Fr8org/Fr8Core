
namespace pluginAzureSqlServer.Services
{
    public interface IAzureSqlServerEvent
    {
        void Process(string curEventPayload);
    }
}