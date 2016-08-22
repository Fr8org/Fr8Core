using System;
using System.Runtime.Serialization;

namespace Fr8.TerminalBase.Errors
{
    public class AuthorizationTokenExpiredOrInvalidException : Exception
    {
        public AuthorizationTokenExpiredOrInvalidException(): base(string.Empty)
        {
        }

        public AuthorizationTokenExpiredOrInvalidException(string message) 
            : base(message)
        {
        }

        public AuthorizationTokenExpiredOrInvalidException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected AuthorizationTokenExpiredOrInvalidException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
