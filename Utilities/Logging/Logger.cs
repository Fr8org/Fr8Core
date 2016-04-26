using System.Diagnostics;
using System.Linq;
using System.Net;
using log4net;
using log4net.Appender;

namespace Utilities.Logging
{

    public enum EventType
    {
        Info,
        Error,
        Warning
    }

    public static class Logger
    {
        static Logger()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static ILog GetLogger(int depth = 1)
        {
            return LogManager.GetLogger(new StackTrace().GetFrame(depth).GetMethod().DeclaringType);
        }

        public static ILog GetPapertrailLogger(string targetPapertrailUrl, int papertrailPort, int depth = 1)
        {
            var curPapertrailAppender =
                LogManager.GetRepository()
                    .GetAppenders()
                    .Single(appender => appender.Name.Equals("PapertrailRemoteSyslogAppender")) as RemoteSyslogAppender;

            curPapertrailAppender.RemoteAddress = Dns.GetHostAddresses(targetPapertrailUrl)[0];
            curPapertrailAppender.RemotePort = papertrailPort;
            curPapertrailAppender.ActivateOptions();

            return LogManager.GetLogger(new StackTrace().GetFrame(depth).GetMethod().DeclaringType);
        }


        #region

        public static void LogMessage(string message, EventType eventType = EventType.Info, int depth = 2)
        {
            var logger = GetLogger(depth);

            switch (eventType)
            {
                case EventType.Info:
                    logger.Info(message);
                    break;
                case EventType.Error:
                    logger.Error(message);
                    break;
                case EventType.Warning:
                    logger.Warn(message);
                    break;
                default:
                {
                    logger.Info(message);
                    break;
                }
            }

        }

        public static void LogInfo(string message)
        {
            LogMessage(message, EventType.Info, 3);
        }

        public static void LogWarning(string message)
        {
            LogMessage(message,EventType.Warning, 3);
        }

        public static void LogError(string message)
        {
            LogMessage(message, EventType.Error, 3);
        }

        #endregion

    }
}