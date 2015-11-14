using System.Diagnostics;
using System.Linq;
using System.Net;
using log4net;
using log4net.Appender;

namespace Utilities.Logging
{
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

        public static ILog GetPapertrialLogger(string targetPapertrialUrl, int papertrialPort, int depth = 1)
        {
            var curPapertrialAppender =
                LogManager.GetRepository()
                    .GetAppenders()
                    .Single(appender => appender.Name.Equals("PapertrailRemoteSyslogAppender")) as RemoteSyslogAppender;

            curPapertrialAppender.RemoteAddress = Dns.GetHostAddresses(targetPapertrialUrl)[0];
            curPapertrialAppender.RemotePort = papertrialPort;
            curPapertrialAppender.ActivateOptions();

            return LogManager.GetLogger(new StackTrace().GetFrame(depth).GetMethod().DeclaringType);
        }
    }
}