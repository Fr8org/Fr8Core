using System.IO;
using Fr8.Infrastructure.Utilities.Configuration;
using log4net.Core;
using log4net.Layout;
using YamlDotNet.Core.Tokens;

namespace Fr8.Infrastructure.Utilities.Logging
{
    public class Fr8PatternLayout : PatternLayout
    {
        private string LoggerPrefix { get; set; }

        public string DebugColor { get; set; }
        public string InfoColor { get; set; }
        public string WarnColor { get; set; }
        public string ErrorColor { get; set; }
        public string FatalColor { get; set; }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            
            //whether we should add full exception to journal -> false = we do not add them / true = add rendered exceptions 
            IgnoresException = true;

            InfoColor = InfoColor.IsNullOrEmpty() ? "\x1b[36m" : "\x1b[" + InfoColor;
            WarnColor = WarnColor.IsNullOrEmpty() ? "\x1b[33m" : "\x1b[" + WarnColor;
            ErrorColor = ErrorColor.IsNullOrEmpty() ? "\x1b[31m" : "\x1b[" + ErrorColor;

            try
            {
                LoggerPrefix = CloudConfigurationManager.GetSetting("LoggerNamePrefix");
            }
            catch
            {
                LoggerPrefix = null;
            }

            if (string.IsNullOrWhiteSpace(LoggerPrefix))
            {
                LoggerPrefix = null;
            }
        }

        public override void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            string ending = "\x1b[0m";

            // Info
            if (loggingEvent.Level.Name.Equals(Level.Info.Name))
            {
                writer.Write(InfoColor);
            }
            // Warning
            else if (loggingEvent.Level.Name.Equals(Level.Warn.Name))
            {
                writer.Write(WarnColor);
            }
            // Error
            else if (loggingEvent.Level.Name.Equals(Level.Error.Name))
            {
                writer.Write(ErrorColor);
            }
            else
            {
                ending = null;
            }

            if (LoggerPrefix != null)
            {
                writer.Write(LoggerPrefix);
            }

            base.Format(writer, loggingEvent);

            if (ending != null)
            {
                writer.Write(ending);
            }
            /*else
            {
                writer.WriteLine();
            }*/
        }
    }
}
