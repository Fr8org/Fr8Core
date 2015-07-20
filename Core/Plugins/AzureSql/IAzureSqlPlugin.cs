namespace Core.Plugins.AzureSql
{
    public interface IAzureSqlPlugin
    {
        void WriteCommand(WriteCommandArgs args);
    }
}
