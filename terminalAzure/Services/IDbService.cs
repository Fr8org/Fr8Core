using terminalAzure.Infrastructure;

namespace terminalAzure.Services
{
    public interface IDbService
    {
        /// <summary>
        /// Write some data to remote database.
        /// </summary>
        /// <param name="args">Connection string, provider name, tables data.</param>
        void WriteCommand(WriteCommandArgs args);
    }
}
