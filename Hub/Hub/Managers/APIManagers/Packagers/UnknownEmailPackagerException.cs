using System;

namespace Hub.Managers.APIManagers.Packagers
{
    public class UnknownEmailPackagerException : ApplicationException
    {
        public UnknownEmailPackagerException()
        {
            
        }

        public UnknownEmailPackagerException(string message)
            : base(message)
        {
            
        }

        public UnknownEmailPackagerException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
    }
}
