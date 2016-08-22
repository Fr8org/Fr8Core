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
    
    public static class Logger
    {
        private static readonly string LoggerPrefix;
        
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

        public static ILog GetLogger(string name = "", int depth = 1)
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

            return GetLogger("",depth);
        } 
    }
}