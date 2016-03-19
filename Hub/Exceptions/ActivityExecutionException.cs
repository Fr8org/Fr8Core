using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Exceptions
{
    public class ActivityExecutionException : Exception
    {
        public string UserErrorMessage { get; private set; }

        public ActivityExecutionException()
        {
        }

        public ActivityExecutionException(string userErrorMessage, string errorMessage = "")
            : base(errorMessage ?? string.Empty)
        {
            UserErrorMessage = userErrorMessage;
        }

        public ActivityExecutionException(string userErrorMessage, string message, Exception innerException)
            : base(message, innerException)
        {
            UserErrorMessage = userErrorMessage;
        }

        protected ActivityExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
