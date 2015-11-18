namespace terminalPapertrail.Interfaces
{
    public interface IPapertrailLogger
    {
        void LogToPapertrail(string papertrailUrl, int portNumber, string logMessage);
    }
}
