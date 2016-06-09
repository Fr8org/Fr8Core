using Fr8.Infrastructure.Utilities.Logging;
using terminalPapertrail.Interfaces;

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