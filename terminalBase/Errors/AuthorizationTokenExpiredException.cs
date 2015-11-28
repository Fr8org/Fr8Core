using System;
using System.Runtime.Serialization;

namespace TerminalBase.Errors
{
    public class AuthorizationTokenExpiredException : Exception
    {
        public AuthorizationTokenExpiredException()
        {
        }

        public AuthorizationTokenExpiredException(string message) 
            : base(message)
        {
        }

        public AuthorizationTokenExpiredException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected AuthorizationTokenExpiredException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
