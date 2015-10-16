using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class AuthenticationExeception : Exception
    {
        public AuthenticationExeception()
        {
        }

        public AuthenticationExeception(string message) 
            : base(message)
        {
        }

        public AuthenticationExeception(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected AuthenticationExeception(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
