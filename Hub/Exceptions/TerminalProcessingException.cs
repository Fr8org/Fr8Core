using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Exceptions
{
    public class TerminalProcessingException : Exception
    {
        public string UserErrorMessage { get; private set; }

        public TerminalProcessingException()
        {
        }

        public TerminalProcessingException(string userErrorMessage, string errorMessage = "")
            : base(errorMessage ?? string.Empty)
        {
            UserErrorMessage = userErrorMessage;
        }

        public TerminalProcessingException(string userErrorMessage, string message, Exception innerException)
            : base(message, innerException)
        {
            UserErrorMessage = userErrorMessage;
        }

        protected TerminalProcessingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
