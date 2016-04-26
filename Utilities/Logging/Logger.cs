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
        static string ErrorColor = "\x1b[31m";
        static string InfoColor = "\x1b[36m";
        static string WarnColor = "\x1b[33m";

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


        #region Solution logging logic 

        static string WrapColor(string message, string color)
        {
            return color + message + "\x1b[0m";
        }

        /// <summary>
        /// Logs message with log4net
        /// </summary>
        /// <param name="message">formatted messsage should, should contain critical data like Fr8UserId</param>
        /// <param name="eventType"></param>
        /// <param name="depth">Defines how many stack frames we slice from top</param>
        public static void LogMessage(string message, EventType eventType = EventType.Info, int depth = 2)
        {
            var logger = GetLogger(depth);

            switch (eventType)
            {
                case EventType.Info:
                    logger.Info(WrapColor(message, InfoColor));
                    break;
                case EventType.Error:
                    logger.Error(WrapColor(message, ErrorColor));
                    break;
                case EventType.Warning:
                    logger.Warn(WrapColor(message, WarnColor));
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
            LogMessage(message, EventType.Warning, 3);
        }

        public static void LogError(string message)
        {
            LogMessage(message, EventType.Error, 3);
        }



        #endregion

    }
}