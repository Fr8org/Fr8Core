using System;

namespace Hub.Exceptions
{
    public class WrongAuthenticationTypeException : AuthenticationExeception
    {
        public WrongAuthenticationTypeException() : base("Terminal doesn't require authentication")
        {
        }

        public WrongAuthenticationTypeException(string message) : base(message)
        {
        }
    }
}