using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Core;
using log4net.Layout;


namespace Utilities.Logging
{
    public class Fr8Log4NetLayout : LayoutSkeleton
    {
        public string DebugColor {get; set; }
        public string InfoColor { get; set; }
        public string WarnColor { get; set; }
        public string ErrorColor { get; set; }
        public string FatalColor { get; set; }

        public override void ActivateOptions()
        {
            IgnoresException = false;
            InfoColor = InfoColor.IsNullOrEmpty() ? "\x1b[36m" : "\x1b[" + InfoColor;
            WarnColor = WarnColor.IsNullOrEmpty() ? "\x1b[33m" : "\x1b[" + WarnColor;
            ErrorColor = ErrorColor.IsNullOrEmpty() ? "\x1b[31m" : "\x1b[" + ErrorColor;
        }

        static string WrapColor(string message, string color)
        {
            return color + message + "\x1b[0m";
        }

        public override void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            string message = loggingEvent.ExceptionObject == null ? 
                loggingEvent.MessageObject.ToString() :
                loggingEvent.MessageObject+ " " + loggingEvent.ExceptionObject.ToString();
            

            // can`t use polymorphism because it is same class
            // can`t use switch because case should be constant
            // can`t use value iteration... f__
            // Info
            if (loggingEvent.Level.Name.Equals(Level.Info.Name))
            {
                message = WrapColor(message,InfoColor);
            }
            // Warning
            if (loggingEvent.Level.Name.Equals(Level.Warn.Name))
            {
                message = WrapColor(message, WarnColor);
            }
            // Error
            if (loggingEvent.Level.Name.Equals(Level.Error.Name))
            {
                message = WrapColor(message, ErrorColor);
            }

            // no config formatting, only hardcode
            var output = $"{loggingEvent.Level.DisplayName} - "+
                         $"{message}";
            
            writer.WriteLine(output);
        }
    }
}
