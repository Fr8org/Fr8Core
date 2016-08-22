using System;
using System.Runtime.Serialization;

namespace Hub.Exceptions
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
