using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Exceptions
{
    public class ErrorResponseException : Exception
    {
        public string CurrentActivityName { get; set; }
        public string CurrentTerminalName { get; set; }

        public ErrorResponseException()
        {
        }

        public ErrorResponseException(string message, string activityName = null, string terminalName = null)
            : base(message)
        {
            CurrentActivityName = activityName;
            CurrentTerminalName = terminalName;
        }

        public ErrorResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ErrorResponseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
