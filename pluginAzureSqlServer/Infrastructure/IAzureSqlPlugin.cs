namespace pluginAzureSqlServer.Infrastructure
{
    public interface IAzureSqlPlugin
    {
        void WriteCommand(WriteCommandArgs args);
    }
}
