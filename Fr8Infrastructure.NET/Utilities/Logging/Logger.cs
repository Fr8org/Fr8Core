using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using Fr8.Infrastructure.Utilities.Configuration;
using log4net;
using log4net.Appender;

namespace Fr8.Infrastructure.Utilities.Logging
{
    public enum EventType
    {
        Info,
        Error,
        Warning
    }

    public static class Logger
    {
        private static readonly string LoggerPrefix;

        static string ErrorColor = "\x1b[31m";
        static string InfoColor = "\x1b[36m";
        static string WarnColor = "\x1b[33m";
        
        static Logger()
        {
            log4net.Config.XmlConfigurator.Configure();
            LoggerPrefix = CloudConfigurationManager.GetSetting("LoggerNamePrefix");

            if (string.IsNullOrWhiteSpace(LoggerPrefix))
            {
                LoggerPrefix = null;
            }
        }

        private static string PrependLoggerPrefix(string loggerName)
        {
            if (LoggerPrefix == null)
            {
                return loggerName;
            }

            return LoggerPrefix + loggerName;
        }


        // taken from NLog  sources
        public static ILog GetCurrentClassLogger()
        {
            string className;
            Type declaringType;
            int framesToSkip = 2;

            do
            {
                StackFrame frame = new StackFrame(framesToSkip, false);
                MethodBase method = frame.GetMethod();

                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    className = method.Name;
                    break;
                }

                framesToSkip++;
                className = declaringType.FullName;
            } while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return LogManager.GetLogger(Assembly.GetCallingAssembly(), PrependLoggerPrefix(className));
        }

        public static ILog GetLogger(int depth = 1, string name = "")
        {
            if (name.IsNullOrEmpty())
            {
                var type = new StackTrace().GetFrame(depth).GetMethod().DeclaringType;
                return LogManager.GetLogger(Assembly.GetCallingAssembly(), PrependLoggerPrefix(type != null ? type.FullName : "<unknown>"));
            }
            
            return LogManager.GetLogger(PrependLoggerPrefix(name));
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

            return GetLogger(depth);
        }


        #region Solution logging logic 

        static string WrapColor(string message, string color)
        {
            return color + message + "\x1b[0m";
        }

        /// <summary>
        /// Logs message with log4net
        /// </summary>
        /// <param name="message">formatted messsage should contain critical data like Fr8UserId</param>
        /// <param name="eventType"></param>
        /// <param name="depth">Defines how many stack frames we slice from top</param>
        public static void LogMessage(string message, EventType eventType = EventType.Info, int depth = 0)
        {
            depth += 3;
            LogMessageWithNamedLogger(message, eventType, depth);
        }

        public static void LogMessageWithNamedLogger(string message, EventType eventType = EventType.Info, int depth = 0, string loggerName = "")
        {
            //if somebody calls LogMessageWithNamedLogger from outside with depth of stack trace we dont want to show Logger calls
            depth += 2;
            var logger = GetLogger(depth, loggerName);

            switch (eventType)
            {
                case EventType.Info:
                    logger.Info(WrapColor(message,InfoColor));
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
            LogMessage(message, EventType.Info, 4);
        }

        public static void LogInfo(string message, string loggerName)
        {
            LogMessageWithNamedLogger(message, EventType.Info, 3, loggerName);
        }

        public static void LogWarning(string message)
        {
            LogMessage(message, EventType.Warning, 4);
        }

        public static void LogWarning(string message, string loggerName)
        {
            LogMessageWithNamedLogger(message, EventType.Warning, 3, loggerName);
        }

        public static void LogError(string message)
        {
            LogMessage(message, EventType.Error, 4);
        }

        public static void LogError(string message, string loggerName)
        {
            LogMessageWithNamedLogger(message, EventType.Error, 3, loggerName);
        }

        #endregion

    }
}