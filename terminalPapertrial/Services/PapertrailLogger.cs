using terminalPapertrail.Interfaces;
using Utilities.Logging;

namespace terminalPapertrail.Services
{
    public class PapertrailLogger : IPapertrailLogger
    {
        public void LogToPapertrail(string papertrailUrl, int portNumber, string logMessage)
        {
            Logger.GetPapertrailLogger(papertrailUrl, portNumber).Info(logMessage);
        }
    }
}